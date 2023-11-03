using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurlyCore.Saving
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SerializerMetadataAttribute : Attribute
    {
        public string ID { get; }
        public SerializationType SerializationFormat { get ;}
        
        public SerializerMetadataAttribute(string id, SerializationType type)
        {
            this.ID = id;
            if (id.Length != 3) throw new Exception($"Serialization ID must be 3 characters! Please change ID '{id}' to be 3 characters!");
            SerializationFormat =  type;
        }
    }

    public class SerializerID : PropertyAttribute
    {
        
    }
}

