using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CurlyCore.CurlyApp;
using UnityEngine;

namespace CurlyCore
{
    public static class DependencyInjector
    {
        public static void InjectDependencies(object target)
        {
            var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                GlobalDefaultAttribute attribute = field.GetCustomAttribute(typeof(GlobalDefaultAttribute)) as GlobalDefaultAttribute;
                if (attribute == null) continue;

                var defaultValue = GlobalDefaultStorage.GetDefault(field.FieldType);

                if (defaultValue != null)
                {
                    field.SetValue(target, defaultValue);
                }
                else
                {
                    Debug.LogError($"DependencyInjector: Default for {field.Name} not registered!");
                }
            }
        }
    }
}
