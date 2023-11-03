using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEditor;

namespace CurlyEditor.Utility
{
    public class AssetUtility
    {
        /// <summary>Gets an array of assets of type T at a given path. This path is relative to /Assets.</summary>
        /// <returns>An array of assets of type T.</returns>
        /// <param name="path">The file path relative to /Assets.</param>
        public static T[] GetAssetsAtPath<T>(string path) where T : Object
        {
            List<T> returnList = new List<T>();

            //get the contents of the folder's full path (excluding any meta files) sorted alphabetically
            IEnumerable<string> fullpaths = Directory.GetFiles(Path.GetFullPath(path)).Where(x => !x.EndsWith(".meta")).OrderBy(s => s);
            //loop through the folder contents
            foreach (string fullpath in fullpaths)
            {
                //determine a path starting with Assets
                string assetPath = UnityEditor.FileUtil.GetProjectRelativePath(fullpath.Replace(@"\", "/"));
                //load the asset at this relative path
                Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
                //and add it to the list if it is of type T
                if (obj is T) { returnList.Add(obj as T); }
            }

            return returnList.ToArray();
        }

        /// <summary>Gets an array of paths of assets of type T at a given path. This path is relative to /Assets.</summary>
        /// <returns>An array of assets of type T.</returns>
        /// <param name="path">The file path relative to /Assets.</param>
        public static string[] GetAssetPathsAtPath<T>(string path) where T : Object
        {
            List<string> returnList = new List<string>();

            //get the contents of the folder's full path (excluding any meta files) sorted alphabetically
            IEnumerable<string> fullpaths = Directory.GetFiles(Path.GetFullPath(path)).Where(x => !x.EndsWith(".meta")).OrderBy(s => s);
            //loop through the folder contents
            foreach (string fullpath in fullpaths)
            {
                //determine a path starting with Assets
                string assetPath = UnityEditor.FileUtil.GetProjectRelativePath(fullpath.Replace(@"\", "/"));
                //load the asset at this relative path
                Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
                //and add it to the list if it is of type T
                if (obj is T) { returnList.Add(assetPath); }
            }

            return returnList.ToArray();
        }

        /// </summary>
        /// <param name="directoryPath">The path of the directory to search in.</param>
        /// <typeparam name="T">The type of assets to find.</typeparam>
        /// <returns>A Dictionary containing the found assets as keys and their corresponding paths as values.</returns>
        public static Dictionary<T, string> GetAssetsInDirectory<T>(string directoryPath) where T : Object
        {
            // This will hold the final result
            Dictionary<T, string> assetDict = new Dictionary<T, string>();

            // Find all assets of type T in the directory
            // The "t:" filter in FindAssets allows us to filter by type
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new string[] { directoryPath });

            // Loop over the results
            foreach (string guid in guids)
            {
                // Get the asset path
                // The GUIDToAssetPath function translates the unique identifier to a relative path
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Load the asset from the path
                // LoadAssetAtPath loads an asset stored at path in a Resources folder
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);

                // If asset is not null, add it to the dictionary
                // This adds the asset and its path to our result dictionary
                if (asset != null)
                {
                    assetDict.Add(asset, path);
                }
            }

            // Return the resulting dictionary
            return assetDict;
        }
    }
}
