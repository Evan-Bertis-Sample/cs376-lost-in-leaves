using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace CurlyCore.Saving
{
    [System.Serializable]
    public struct SaveHeader
    {
        [field: SerializeField] public string SerializerID;// Must be 3 characters -- 3 bytes
        [field: SerializeField] public SerializationType SerializationType; // 4 bytes
        [field: SerializeField] public DateTime TimeSaved; // 8 bytes

        public SaveHeader(string id, SerializationType type, DateTime time = default)
        {
            if (id.Length != 3) throw new Exception($"Cannot make SaveHeader: Serialization ID must be 3 characters! Please change ID '{id}' to be 3 characters!");
            SerializerID = id;
            SerializationType = type;
            TimeSaved = (time == default) ? DateTime.Now : time;
        }

        public static SaveHeader Deserialize(Byte[] bytes, SerializationType type)
        {
            switch (type)
            {
                case SerializationType.TEXT:
                    return DeserializeFromText(bytes);
                case SerializationType.BINARY:
                    return DeserializeFromBinary(bytes);
                default:
                    throw new Exception("Invalid SerializationType!");
            }
        }

        private static SaveHeader DeserializeFromText(Byte[] bytes)
        {
            string textRepresentation = Encoding.UTF8.GetString(bytes);
            SaveHeader deserializedHeader = JsonConvert.DeserializeObject<SaveHeader>(textRepresentation);
            return deserializedHeader;
        }

        private static SaveHeader DeserializeFromBinary(Byte[] bytes)
        {
            if (bytes.Length != GetBinarySerializationByteSize()) throw new Exception("Invalid byte array length for binary deserialization!");
            string id = Encoding.ASCII.GetString(bytes, 0, 3);
            SerializationType format = (SerializationType)BitConverter.ToInt32(bytes, 3);
            DateTime time = DateTime.FromBinary(BitConverter.ToInt64(bytes, 7));
            return new SaveHeader(id, format, time);
        }

        public static int GetByteSize(SerializationType type)
        {
            switch (type)
            {
                case SerializationType.BINARY:
                    return GetBinarySerializationByteSize();
                case SerializationType.TEXT:
                    return GetTextSerializationByteSize();
                default:
                    throw new System.Exception("Invalid type!");
            }
        }

        private static int GetBinarySerializationByteSize()
        {
            SaveHeader fake = new SaveHeader("FKE", SerializationType.BINARY);
            return fake.Serialize().Length;
        }

        // Just a quick way to get the length of metadata
        private static int GetTextSerializationByteSize()
        {
            SaveHeader fake = new SaveHeader("FKE", SerializationType.TEXT);
            return fake.Serialize().Length;
        }

        public Byte[] Serialize()
        {
            switch (SerializationType)
            {
                case SerializationType.TEXT:
                    return SerializeAsText();
                case SerializationType.BINARY:
                    return SerializeAsBinary();
                default:
                    throw new Exception("Invalid SerializationType!");
            }
        }

        public override string ToString()
        {
            return $"SaveHeader: SerializerID = {SerializerID}, SerializationType = {SerializationType}, TimeSaved = {TimeSaved}";
        }

        private Byte[] SerializeAsText()
        {
            string textRepresentation = JsonConvert.SerializeObject(this, Formatting.Indented);
            textRepresentation += "\n";
            return Encoding.UTF8.GetBytes(textRepresentation);
        }

        private Byte[] SerializeAsBinary()
        {            
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(Encoding.UTF8.GetBytes(SerializerID));
                ms.Write(BitConverter.GetBytes((int)SerializationType)); 
                ms.Write(BitConverter.GetBytes(TimeSaved.ToBinary()));
                
                return ms.ToArray();
            }
        }

    }
}
