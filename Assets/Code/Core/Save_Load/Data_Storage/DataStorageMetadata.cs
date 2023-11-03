using System;
using UnityEngine;

namespace CurlyCore.Saving
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DataStorageMetadata : Attribute
    {
        public string ID { get; private set; }

        public DataStorageMetadata(string id)
        {
            ID = id;
        }
    }

    public class DataStorageID : PropertyAttribute
    {

    }
}
