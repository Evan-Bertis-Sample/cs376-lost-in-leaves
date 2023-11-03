using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CurlyCore.Saving
{
    public static class DataStorageFactory
    {
        private static readonly Dictionary<string, Type> _storage = new Dictionary<string, Type>();

        static DataStorageFactory()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IDataStorage).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<DataStorageMetadata>();
                if (attribute != null)
                {
                    _storage[attribute.ID] = type;
                }
            }
        }

        public static IDataStorage CreateStorage(string storageID)
        {
            if (_storage.TryGetValue(storageID, out Type type))
            {
                return (IDataStorage)Activator.CreateInstance(type);
            }

            throw new InvalidOperationException("Unknown storage type.");
        }

        public static List<string> GetStorageIDs() => _storage.Keys.ToList();
    }
}
