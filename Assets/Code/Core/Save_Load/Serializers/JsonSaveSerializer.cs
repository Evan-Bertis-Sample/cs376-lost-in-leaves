using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


namespace CurlyCore.Saving
{
    [SerializerMetadata("JSN", SerializationType.TEXT)]
    public class JsonSaveSerializer : ISaveDataSerializer
    {
        public SaveData Load(Byte[] data)
        {
            Debug.Log("loading");
            string json = System.Text.Encoding.UTF8.GetString(data);
            Debug.Log(json);
            return JsonConvert.DeserializeObject<SaveData>(json);
        }

        public byte[] Save(SaveData toSave)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };

            settings.Converters.Insert(0, new FactDictionaryJsonConverter());

            string jsonData = JsonConvert.SerializeObject(toSave, settings);

            return System.Text.Encoding.UTF8.GetBytes(jsonData);
        }
    }

    /// <summary>
    /// This is an auxiliary JsonConverter that preserves type and assembly information for serialized FactDictionary.
    /// This extra data is necessary because some primitives and value types lose type info when serialized due to JSON spec.
    /// When they are deserialized, they may be misinterpreted as a different type to maximize compatibility (i.e. int -> long, Guid -> string).
    /// This causes a lot of casting problems down the line when we try to fetch the data.
    /// 
    /// Source code: https://stackoverflow.com/questions/25007001/json-net-does-not-preserve-primitive-type-information-in-lists-or-dictionaries-o
    /// </summary>
    public sealed class FactDictionaryJsonConverter : JsonConverter
    {
        public FactDictionaryJsonConverter()
        {

        }

        public override bool CanRead => false; // this is a write-only converter

        public override bool CanConvert(Type objectType)
        {
            // primitives: int, float, long, etc
            // value types: structs, enums
            return objectType.IsPrimitive || objectType.IsValueType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // this is a write-only converter
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch (serializer.TypeNameHandling)
            {
                case TypeNameHandling.All:
                    writer.WriteStartObject();
                    writer.WritePropertyName("$type", false);

                    switch (serializer.TypeNameAssemblyFormatHandling)
                    {
                        case TypeNameAssemblyFormatHandling.Full:
                            writer.WriteValue(value.GetType().AssemblyQualifiedName);
                            break;
                        case TypeNameAssemblyFormatHandling.Simple:
                            string typeName = value.GetType().FullName + ", " + value.GetType().Assembly.GetName().Name;
                            writer.WriteValue(typeName);
                            break;
                        // not including assembly names is apparently a recipe for disaster
                        default:
                            writer.WriteValue(value.GetType().FullName);
                            break;
                    }

                    writer.WritePropertyName("$value", false);
                    try
                    {
                        writer.WriteValue(value);
                    }
                    catch
                    {
                        // ok so structs are value types, right, but some can't be written as a single JSON token
                        // what we do here is break them down into their fields and write them individually
                        writer.WriteStartObject();
                        foreach (var field in value.GetType().GetFields())
                        {
                            writer.WritePropertyName(field.Name);
                            writer.WriteValue(field.GetValue(value));
                        }
                        writer.WriteEndObject();
                    }
                    writer.WriteEndObject();
                    break;
                default:
                    writer.WriteValue(value);
                    break;
            }
        }
    }

}

