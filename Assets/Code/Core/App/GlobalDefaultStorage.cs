using System;
using System.Collections;
using System.Collections.Generic;
using CurlyCore.CurlyApp;
using UnityEngine;

namespace CurlyCore
{
    public static class GlobalDefaultStorage
    {
        private static Dictionary<Type, object> _defaults = new Dictionary<Type, object>();

        public static void RegisterDefault(Type type, object value)
        {
            _defaults[type] = value;
        }

        public static object GetDefault(Type type)
        {
            if (_defaults.Count == 0)
            {
                // None of the defaults have been registered through the app -- this is the inspector
                App.Instance.RegisterDefaults();
            }

            if (_defaults.TryGetValue(type, out object value))
            {
                return value;
            }

            throw new InvalidOperationException($"GlobalDefaultStorage : No assigned default for {type.AssemblyQualifiedName}");
        }
    }
}
