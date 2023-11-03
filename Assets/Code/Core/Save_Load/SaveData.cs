using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CurlyCore.CurlyApp;
using CurlyCore.Debugging;

namespace CurlyCore.Saving
{
    [System.Serializable]
    public class SaveData
    {
        public FactDictionary Facts;

        [GlobalDefault] private GroupLogger _logger;

        public SaveData()
        {
            Facts = new FactDictionary();
        }

        public SaveData(FactDictionary facts)
        {
            Facts = facts;
        }

        public void Save<T>(string factname, T value)
        {
            if (_logger == null) DependencyInjector.InjectDependencies(this);
            try
            {
                T val = value;
                Facts[factname] = value;
            }
            catch
            {
                _logger.Log(LoggingGroupID.APP, $"Unable to store fact {factname}. Check if this fact is already being stored under a different type");
            }
        }

        public T Load<T>(string factName, T fallback)
        {
            if (_logger == null) DependencyInjector.InjectDependencies(this);
            try
            {
                return (T)Facts[factName];
            }
            catch (System.Exception error)
            {
                _logger.Log(LoggingGroupID.APP, error, LogType.Warning);
                return fallback;
            }
        }
    }
}
