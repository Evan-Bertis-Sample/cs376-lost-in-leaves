using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurlyCore.Saving
{
    public interface ISaveDataSerializer
    {
        public SaveData Load(Byte[] data);
        public Byte[] Save(SaveData toSave);
    }
}

