using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurlyCore.Saving
{
    public interface IDataStorage
    {
        public void StoreData(string dataKey, byte[] data);
        public byte[] RetrieveData(string dataKey);
        public List<string> GetAllKeys();
    }
}
