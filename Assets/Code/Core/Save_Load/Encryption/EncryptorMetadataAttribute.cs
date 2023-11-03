using System;
using UnityEngine;

namespace CurlyCore.Saving
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EncryptorMetadataAttribute : Attribute
    {
        public string ID { get; }
        
        public EncryptorMetadataAttribute(string id)
        {
            this.ID = id;
        }
    }

    public class EncryptorID : PropertyAttribute
    {

    }
}
