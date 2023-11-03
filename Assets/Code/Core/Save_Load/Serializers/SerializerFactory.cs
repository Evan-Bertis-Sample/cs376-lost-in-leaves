using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CurlyCore.Saving
{
    public class SerializerFactory
    {
        private static readonly Dictionary<string, Type> _serializers = new Dictionary<string, Type>();

        static SerializerFactory()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ISaveDataSerializer).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<SerializerMetadataAttribute>();
                if (attribute != null)
                {
                    _serializers[attribute.ID] = type;
                }
            }
        }

        public static ISaveDataSerializer CreateSerializer(string serializerID)
        {
            if (_serializers.TryGetValue(serializerID, out Type type))
            {
                return (ISaveDataSerializer)Activator.CreateInstance(type);
            }

            throw new InvalidOperationException("Unknown serializer type.");
        }

        public static List<string> GetSerializerIDs() => _serializers.Keys.ToList();
    }
}