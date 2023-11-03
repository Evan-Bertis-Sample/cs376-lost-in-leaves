using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using CurlyEditor.Utility;

namespace CurlyEditor.Core.SceneManagement
{
    public class SceneAdder
    {
        public static void SetBuildScenes(string path)
        {
            SceneAsset[] scenes = GetScenesAtPath(path);
            List<EditorBuildSettingsScene> newBuildScenes = new List<EditorBuildSettingsScene>();
            for (int i = 0; i < scenes.Length; i++)
            {
                newBuildScenes.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(scenes[i]), true));
            }
            Debug.Log($"Committing {newBuildScenes.Count} scenes to Build");
            EditorBuildSettings.scenes = newBuildScenes.ToArray();
        }

        private static SceneAsset[] GetScenesAtPath(string path)
        {
            SceneAsset[] sceneAssets = AssetUtility.GetAssetsAtPath<SceneAsset>(path);
            Debug.Log($"Found {sceneAssets.Length} assets at '{path}'");
            return sceneAssets;
        }
    }
}