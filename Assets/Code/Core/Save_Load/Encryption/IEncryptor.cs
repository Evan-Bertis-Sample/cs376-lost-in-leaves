using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurlyCore.Saving
{
    public interface IEncryptor
    {
        public void SetKeyAndIV(byte[] key, byte[] iv);
        public byte[] Encrypt(byte[] data);
        public byte[] Decrypt(byte[] data);
    }
}
