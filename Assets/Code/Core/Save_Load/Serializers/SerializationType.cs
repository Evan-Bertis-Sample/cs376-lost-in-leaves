using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurlyCore.Saving
{
    // Serialization will assume that a char is encoded using UTF-8, the standard for writing files in C#
    public enum SerializationType
    {
        BINARY,
        TEXT
    }
}
