using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


namespace LostInLeavesEditor.Tools
{
    /// <summary>
    /// Terrain tools for adding trees to a terrain
    /// Curtosy of https://forum.unity.com/threads/easily-add-many-trees-to-your-terrain.225679/
    /// </summary>
    public static class TerrainTools
    {
        [MenuItem("Terrain/Add Folder Tree")]
        static void AddFolderTrees()
        {
            string folder = EditorUtility.OpenFolderPanel("Select the folder containing the tree", "Assets/", "");
            if (folder != "")
            {
                if (folder.IndexOf(Application.dataPath) == -1)
                {
                    Debug.LogWarning("The folder must be in this project anywhere inside the Assets folder!");
                    return;
                }
                string[] files = Directory.GetFiles(folder);
                if (files.Length > 0)
                {
                    TerrainData currentTerrainData = Selection.activeGameObject.GetComponent<Terrain>().terrainData;
                    List<TreePrototype> treePrototypesList = new List<TreePrototype>(currentTerrainData.treePrototypes);
                    for (int i = 0; i < files.Length; i++)
                    {

                        TreePrototype treePrototype = new TreePrototype();
                        string relativePath = files[i].Substring(files[i].IndexOf("Assets/"));
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
                        if (prefab != null)
                        {
                            treePrototype.prefab = prefab;
                            treePrototypesList.Add(treePrototype);
                        }
                    }
                    currentTerrainData.treePrototypes = treePrototypesList.ToArray();
                    Selection.activeGameObject.GetComponent<Terrain>().Flush();
                    currentTerrainData.RefreshPrototypes();
                    EditorUtility.SetDirty(Selection.activeGameObject.GetComponent<Terrain>());
                }
            }
        }

        [MenuItem("Terrain/Clear Tree Editor")]
        static void ClearTreeEditor()
        {
            TerrainData currentTerrainData = Selection.activeGameObject.GetComponent<Terrain>().terrainData;
            currentTerrainData.treePrototypes = null;
            Selection.activeGameObject.GetComponent<Terrain>().Flush();
            currentTerrainData.RefreshPrototypes();
            EditorUtility.SetDirty(Selection.activeGameObject.GetComponent<Terrain>());
        }

        [MenuItem("Terrain/Add Folder Tree", true)]
        static bool ValidateAddFolderTrees()
        {
            if (Selection.activeGameObject == null || Selection.activeGameObject.GetComponent<Terrain>() == null)
            {
                Debug.LogWarning("You must have a Terrain selected to perform this action!");
                return false;
            }
            return true;
        }

        [MenuItem("Terrain/Clear Tree Editor", true)]
        static bool ValidateClearTreeEditor()
        {
            if (Selection.activeGameObject == null || Selection.activeGameObject.GetComponent<Terrain>() == null)
            {
                Debug.LogWarning("You must have a Terrain selected to perform this action!");
                return false;
            }
            return true;
        }
    }
}
