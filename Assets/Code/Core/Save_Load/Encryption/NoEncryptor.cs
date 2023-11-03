using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurlyCore.Saving
{
    [EncryptorMetadata("NONE")]
    public class NoEncryptor : IEncryptor
    {
        public byte[] Decrypt(byte[] data)
        {
            return data;
        }

        public byte[] Encrypt(byte[] data)
        {
            return data;
        }

        public void SetKeyAndIV(byte[] key, byte[] iv)
        {
            return;
        }
    }
}
