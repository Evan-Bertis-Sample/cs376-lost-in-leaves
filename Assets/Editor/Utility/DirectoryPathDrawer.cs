using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using CurlyUtility;

namespace CurlyEditor.Utility
{
    [CustomPropertyDrawer(typeof(DirectoryPath))]
    public class DirectoryPathDrawer : SearchBarDrawer
    {
        protected override void ButtonClicked(Rect buttonPosition)
        {
            DirectoryPath pathAttribute = attribute as DirectoryPath;
            string absolute = EditorUtility.OpenFolderPanel("Choose Directory", "", "");
            _propertyValue = (pathAttribute.IsAbsolutePath) ? absolute : FileUtil.GetProjectRelativePath(absolute);
        }
    }

    [CustomPropertyDrawer(typeof(FilePath))]
    public class FilePathDrawer : SearchBarDrawer
    {
        protected override void ButtonClicked(Rect buttonPosition)
        {
            FilePath pathAttribute = attribute as FilePath;
            string absolute = EditorUtility.OpenFilePanel("Choose File", "", "");
            _propertyValue = (pathAttribute.IsAbsolutePath) ? absolute : FileUtil.GetProjectRelativePath(absolute);
        }
    }
}
