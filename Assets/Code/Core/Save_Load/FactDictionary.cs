using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;

namespace CurlyCore.Saving
{
    [JsonDictionary(ItemTypeNameHandling = TypeNameHandling.All)]
    public class FactDictionary : Dictionary<string, object>
    {
        public void Add(string key, object value)
        {
            base.Add(key, value);
        }

        public T Get<T>(string key)
        {
            if (TryGetValue(key, out object value))
            {
                return (T)value;
            }
            else
            {
                throw new KeyNotFoundException($"Key '{key}' not found in the dictionary.");
            }
        }
    }
}
