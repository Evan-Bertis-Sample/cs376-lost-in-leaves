using System;
using System.IO;
using System.Security.Cryptography;

namespace CurlyCore.Saving
{
    [EncryptorMetadata("AES")]
    public class AesEncryptor : IEncryptor
    {
        private byte[] _key; // 256 bits key
        private byte[] _iv;  // 128 bits initialization vector

        public void SetKeyAndIV(byte[] key, byte[] iv)
        {
            if (key == null || key.Length != 32)
                throw new ArgumentException("Key must be 256 bits long.", nameof(key));

            if (iv == null || iv.Length != 16)
                throw new ArgumentException("IV must be 128 bits long.", nameof(iv));

            _key = key;
            _iv = iv;
        }

        public byte[] Encrypt(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                    }

                    return memoryStream.ToArray();
                }
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                    }

                    return memoryStream.ToArray();
                }
            }
        }
    }
}