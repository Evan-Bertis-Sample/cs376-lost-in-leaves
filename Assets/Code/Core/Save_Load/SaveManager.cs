using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using CurlyCore.CurlyApp;
using System.ComponentModel;

namespace CurlyCore.Saving
{
    [CreateAssetMenu(menuName = "Curly/Core/Save Manager", fileName = "SaveManager")]
    public class SaveManager : RuntimeScriptableObject
    {
        public SaveData CurrentSave { get; private set; }
        [field: SerializeField, DataStorageID] public string DefaultStorage {get; private set;}
        [field: SerializeField, SerializerID] public string DefaultSerializer {get; private set;}
        [field: SerializeField, EncryptorID] public string DefaultEncryptor {get; private set;}

        public IDataStorage Storage => DataStorageFactory.CreateStorage(DefaultStorage);
        public ISaveDataSerializer Serializer => SerializerFactory.CreateSerializer(DefaultSerializer);
        public IEncryptor Encryptor => EncryptorFactory.CreateEncryptorFromID(DefaultEncryptor);

        // Runs on boot
        public override void OnBoot(App app, Scene scene)
        {
            // SaveData data = new SaveData();
            // data.Save("TestFact", 3);

            // SaveUsingDefault(data, "test-default");
            // SaveData load = LoadUsingDefault("test-default");
            // int testLoad = load.Load("TestFact", -1);
            // Debug.Log($"Loaded {testLoad}");

            DependencyInjector.InjectDependencies(this);
        }

        public void SaveUsingDefault(SaveData data, string fileName)
        {
            IDataStorage storage = DataStorageFactory.CreateStorage(DefaultStorage);
            ISaveDataSerializer serializer = SerializerFactory.CreateSerializer(DefaultSerializer);
            IEncryptor encryptor = EncryptorFactory.CreateEncryptorFromID(DefaultEncryptor);
            Save(data, storage, serializer, encryptor, fileName);
        }

        public void Save(SaveData data, IDataStorage storage, ISaveDataSerializer serializer, IEncryptor encryptor = null, string fileName = "")
        {
            // Get the type of the serializer
            Type serializerType = serializer.GetType();
            // Get the SerializerMetadataAttribute from the type
            SerializerMetadataAttribute attribute = serializerType.GetCustomAttribute<SerializerMetadataAttribute>();

            // Check if the attribute is null (i.e., not present on the type)
            if (attribute == null)
            {
                throw new Exception("The provided serializer does not have the required SerializerMetadataAttribute!");
            }

            if (fileName == "") fileName = Guid.NewGuid().ToString();

            byte[] saveEncrypted;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);

                SaveHeader header = new SaveHeader(attribute.ID, attribute.SerializationFormat);
                byte[] headerEncoding = header.Serialize();

                byte[] dataEncoding = serializer.Save(data);

                if (encryptor == null) encryptor = new NoEncryptor();

                writer.Write((int)attribute.SerializationFormat);
                ms.Write(headerEncoding);
                ms.Write(dataEncoding);
                saveEncrypted = encryptor.Encrypt(ms.ToArray());
            }

            byte[] encryptorTag = EncryptorFactory.GetTag(encryptor);
            int length = encryptorTag.Length;
            byte[] tagLengthBytes = BitConverter.GetBytes(length);

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(tagLengthBytes);
                ms.Write(encryptorTag);
                ms.Write(saveEncrypted);

                storage.StoreData(fileName, ms.ToArray());
            }
        }

        public SaveData LoadUsingDefault(string fileName)
        {
            IDataStorage storage = DataStorageFactory.CreateStorage(DefaultStorage);
            return Load(storage, fileName);
        }

        public SaveData Load(IDataStorage storage, string fileName)
        {
            byte[] data = storage.RetrieveData(fileName);

            using (MemoryStream retrievedStream = new MemoryStream(data))
            {
                BinaryReader fileReader = new BinaryReader(retrievedStream);
                int encryptonTagSize = fileReader.ReadInt32();
                byte[] encryptonTag = fileReader.ReadBytes(encryptonTagSize);
                IEncryptor encryptor = EncryptorFactory.CreateEncryptorFromTag(encryptonTag);

                int encryptedDataSize = (int)retrievedStream.Length - encryptonTagSize;

                byte[] dataEncrypted = fileReader.ReadBytes(encryptedDataSize);
                byte[] dataDecrypted = encryptor.Decrypt(dataEncrypted);

                using (MemoryStream saveDataStream = new MemoryStream(dataDecrypted))
                {
                    BinaryReader dataReader = new BinaryReader(saveDataStream);
                    SerializationType format = (SerializationType)dataReader.ReadInt32();
                    int headerSize = SaveHeader.GetByteSize(format);
                    byte[] headerBuffer = dataReader.ReadBytes(headerSize);
                    
                    int bytesRead = headerBuffer.Length;
                    if (bytesRead != headerBuffer.Length)
                    {
                        throw new Exception("Header information is not expected length!");
                    }

                    SaveHeader header = SaveHeader.Deserialize(headerBuffer, format);
                    ISaveDataSerializer serializer = SerializerFactory.CreateSerializer(header.SerializerID);

                    int dataSize = (int)saveDataStream.Length - (int)saveDataStream.Position;
                    byte[] dataBuffer = new byte[dataSize];
                    saveDataStream.Read(dataBuffer, 0, dataSize);
                    SaveData saveData = serializer.Load(dataBuffer);
                    return saveData;
                }
            }
        }
    }
}
