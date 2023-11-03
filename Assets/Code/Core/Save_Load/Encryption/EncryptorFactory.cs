using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace CurlyCore.Saving
{
    public static class EncryptorFactory
    {
        private static readonly Dictionary<string, Type> _encryptors = new Dictionary<string, Type>();
        private static readonly Dictionary<string, string> _tagToID = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _idToTag = new Dictionary<string, string>();

        static EncryptorFactory()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IEncryptor).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<EncryptorMetadataAttribute>();
                if (attribute != null)
                {
                    _encryptors[attribute.ID] = type;
                    IEncryptor encryptor = CreateEncryptorFromID(attribute.ID);
                    SetKeyAndIV(encryptor);
                    byte[] idBytes = System.Text.Encoding.UTF8.GetBytes(attribute.ID);
                    byte[] bytesEncrypted = encryptor.Encrypt(idBytes);
                    string tag = System.Text.Encoding.UTF8.GetString(bytesEncrypted);
                    _tagToID[tag] = attribute.ID;
                    _idToTag[attribute.ID] = tag;
                }
            }
        }

        public static IEncryptor CreateEncryptorFromID(string serializerID)
        {
            if (_encryptors.TryGetValue(serializerID, out Type type))
            {
                IEncryptor encryptor = (IEncryptor)Activator.CreateInstance(type);
                SetKeyAndIV(encryptor);
                return encryptor;
            }

            throw new InvalidOperationException("Unknown encryptor type.");
        }

        public static IEncryptor CreateEncryptorFromTag(byte[] tagBytes)
        {
            string tag = System.Text.Encoding.UTF8.GetString(tagBytes);
            string id = _tagToID[tag];
            return CreateEncryptorFromID(id);
        }

        public static byte[] GetTag(IEncryptor encryptor)
        {
            var attribute = encryptor.GetType().GetCustomAttribute<EncryptorMetadataAttribute>();
            if (attribute == null)
            {
                throw new Exception("Encrytpor is unregistered. Add an EncryptorMetadataAttribute to register the encryptor");
            }

            string tag = _idToTag[attribute.ID];
            return System.Text.Encoding.UTF8.GetBytes(tag);
        }

        public static List<string> GetEncryptorIDs() => _idToTag.Keys.ToList();

        private static void SetKeyAndIV(IEncryptor encryptor)
        {
            // Use the type's fully qualified name as part of the passphrase
            string typeName = encryptor.GetType().FullName;

            // Retrieve the EncryptorMetadataAttribute for the additional ID
            var attribute = encryptor.GetType().GetCustomAttribute<EncryptorMetadataAttribute>();
            if (attribute == null)
            {
                throw new Exception("Encryptor is unregistered. Add an EncryptorMetadataAttribute to register the encryptor.");
            }

            // Combine the type name and the ID to form the passphrase
            string passphrase = typeName + attribute.ID;

            // Hash the passphrase using SHA-256
            byte[] hashedPassphrase = SHA256Hash(passphrase);
            string salt = "CurlyCore.Saving Encryption is simply meant to defer the 99% who want to tamper save files. If someone manages to figure it out, they deserve it.";

            // Generate Key and IV using the hashed passphrase and a static salt (e.g., "YourGameTitle")
            var (key, iv) = GenerateKeyAndIV(hashedPassphrase, salt);

            encryptor.SetKeyAndIV(key, iv);
        }

        private static byte[] SHA256Hash(string data)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                return sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
            }
        }

        private static (byte[] Key, byte[] IV) GenerateKeyAndIV(byte[] passphrase, string salt)
        {
            // Use a key derivation function like PBKDF2 to generate bytes.
            using (var deriveBytes = new Rfc2898DeriveBytes(passphrase, System.Text.Encoding.UTF8.GetBytes(salt), 1000))
            {
                byte[] key = deriveBytes.GetBytes(32);  // For a 256-bit key
                byte[] iv = deriveBytes.GetBytes(16);   // For a 128-bit IV

                return (key, iv);
            }
        }
    }
}
