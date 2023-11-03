using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

using UnityEngine;

namespace CurlyCore.Saving
{
    [SerializerMetadata("BIN", SerializationType.BINARY)]
    public class BinarySaveSerializer : ISaveDataSerializer
    {
        // This is a helper class for the serialization process
        [System.Serializable]
        public class TypeMapper
        {
            [SerializeField] private Dictionary<string, int> _typeToIdMap = new Dictionary<string, int>();
            [SerializeField] private Dictionary<int, string> _idToTypeMap = new Dictionary<int, string>();

            private static BinaryFormatter _formatter = new BinaryFormatter();

            public static TypeMapper Deserialize(byte[] mapping)
            {
                using (MemoryStream ms = new MemoryStream(mapping))
                {
                    TypeMapper mappper = new TypeMapper();
                    BinaryReader reader = new BinaryReader(ms);
                    while (ms.Position < ms.Length)
                    {
                        int id = reader.ReadInt32();
                        string typename = reader.ReadString();
                        mappper._idToTypeMap[id] = typename;
                        mappper._typeToIdMap[typename] = id;
                    }

                    return mappper;
                }
            }

            public byte[] SerializeTypeMapping()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryWriter writer = new BinaryWriter(ms);
                    foreach (var pair in _idToTypeMap)
                    {
                        writer.Write(pair.Key);
                        writer.Write(pair.Value);
                    }
                    return ms.ToArray();
                }
            }

            public int GetTypeIdentifier(Type type)
            {
                string typename = type.AssemblyQualifiedName;
                if (_typeToIdMap.ContainsKey(typename))
                    return _typeToIdMap[typename];

                int newId = _typeToIdMap.Count;
                _typeToIdMap[typename] = newId;
                _idToTypeMap[newId] = typename;
                return newId;
            }

            public Type GetTypeById(int id)
            {
                string typename = _idToTypeMap[id];
                return Type.GetType(typename);
            }
        }

        SaveData ISaveDataSerializer.Load(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                // Deserialize the mapper
                // Get mapper length
                byte[] mapperLengthBuffer = new byte[4];
                ms.Read(mapperLengthBuffer, 0, 4);
                int mapperLength = BitConverter.ToInt32(mapperLengthBuffer);
                // Now get the next how many bytes that represent the mapper
                byte[] mapperBuffer = new byte[mapperLength];
                ms.Read(mapperBuffer, 0, mapperLength);
                TypeMapper mapper = TypeMapper.Deserialize(mapperBuffer);

                // Deserialize the FactDictionary
                byte[] dictLengthBuffer = new byte[4];
                ms.Read(dictLengthBuffer, 0, 4);
                int dictLength = BitConverter.ToInt32(dictLengthBuffer);
                byte[] dictBytes = new byte[dictLength];
                ms.Read(dictBytes, 0, dictLength);
                var factDict = DeserializeFactDictionary(dictBytes, mapper);

                SaveData saveData = new SaveData(factDict);
                Debug.Log(saveData.Load("TestFact", -1));
                return saveData;
            }
        }

        byte[] ISaveDataSerializer.Save(SaveData toSave)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Serialize the FactDictionary
                TypeMapper mapper = new TypeMapper();
                byte[] factDictBytes = SerializeFactDictionary(toSave.Facts, mapper);

                // Serialize the mapper first
                byte[] mapperBytes = mapper.SerializeTypeMapping();
                int mapperSize = mapperBytes.Length;
                ms.Write(BitConverter.GetBytes(mapperSize), 0, 4);
                ms.Write(mapperBytes, 0, mapperBytes.Length);

                // Then write the factDict bytes
                ms.Write(BitConverter.GetBytes(factDictBytes.Length), 0, 4);
                ms.Write(factDictBytes, 0, factDictBytes.Length);

                // // Serialize the remaining SaveData
                // BinaryFormatter formatter = new BinaryFormatter();
                // formatter.Serialize(ms, toSave);

                return ms.ToArray();
            }
        }

        // In your serializer:
        private byte[] SerializeFactDictionary(FactDictionary factDict, TypeMapper mapper)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);

                // Serialize each fact
                writer.Write(factDict.Count);
                foreach (var pair in factDict)
                {
                    writer.Write(pair.Key); // Write the key

                    int typeId = mapper.GetTypeIdentifier(pair.Value.GetType());
                    writer.Write(typeId); // Write the type ID

                    // This is simplistic; you'd ideally have a more complex serializer here.
                    var bytesForObject = ObjectToBytes(pair.Value);
                    writer.Write(bytesForObject.Length);
                    writer.Write(bytesForObject);
                }

                return ms.ToArray();
            }
        }

        // This is a placeholder; replace with actual object-to-bytes serialization logic.
        private byte[] ObjectToBytes(object obj)
        {
            if (obj == null)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                string json = JsonConvert.SerializeObject(obj);
                byte[] dataArray = System.Text.Encoding.UTF8.GetBytes(json);
                ms.Write(dataArray);
                return ms.ToArray();
            }
        }

        private FactDictionary DeserializeFactDictionary(byte[] bytes, TypeMapper mapper)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                BinaryReader reader = new BinaryReader(ms);
                // Deserialize each fact
                FactDictionary factDict = new FactDictionary();
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    string key = reader.ReadString();
                    int typeId = reader.ReadInt32();
                    Type objectType = mapper.GetTypeById(typeId);

                    // This is simplistic; you'd ideally have a more complex deserializer here.
                    int objectLength = reader.ReadInt32();
                    byte[] objectBytes = reader.ReadBytes(objectLength);
                    object objValue = BytesToObject(objectBytes, objectType);

                    factDict.Add(key, objValue);
                }

                return factDict;
            }
        }

        // This is a placeholder; replace with actual bytes-to-object deserialization logic.
        private object BytesToObject(byte[] bytes, Type targetType)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                string json = System.Text.Encoding.UTF8.GetString(bytes);
                return JsonConvert.DeserializeObject(json, targetType);
            }
        }
    }
}