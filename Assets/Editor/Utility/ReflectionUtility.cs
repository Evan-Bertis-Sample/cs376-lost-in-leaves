using UnityEngine;
using System.Reflection;

namespace CurlyEditor.Utility
{
    public static class ReflectionUtility
    {
        public static T GetFieldByType<T>(object target) where T : class
        {
            FieldInfo[] fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(T))
                {
                    return field.GetValue(target) as T;
                }
            }
            return null;
        }
    }
}