using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurlyUtility
{
    public class DirectoryPath : PropertyAttribute
    {
        public bool IsAbsolutePath { get; }

        public DirectoryPath(bool isAbsolutePath = false)
        {
            IsAbsolutePath = isAbsolutePath;
        }
    }

    public class FilePath : PropertyAttribute
    {
        public bool IsAbsolutePath { get; }

        public FilePath(bool isAbsolutePath = false)
        {
            IsAbsolutePath = isAbsolutePath;
        }
    }
}
