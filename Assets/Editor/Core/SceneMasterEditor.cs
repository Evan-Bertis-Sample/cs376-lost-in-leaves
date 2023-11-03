using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

using CurlyCore.SceneManagement;
using CurlyEditor.Utility;
using CurlyUtility;

namespace CurlyEditor.Core.SceneManagement
{
    [CustomEditor(typeof(SceneMaster))]
    public class SceneMasterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SceneMaster sm = target as SceneMaster;
            DrawBuildFileSelect(sm);
            base.OnInspectorGUI();
        }

        public void DrawBuildFileSelect(SceneMaster sm)
        {
            EditorGUILayout.LabelField("Build Scene Path", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUILayout.TextField(sm.ScenePath);
            if (GUILayout.Button("Select"))
            {
                string absolute = EditorUtility.SaveFolderPanel("Select Build Scene Directory", Application.dataPath, "");
                sm.ScenePath = FileUtil.GetProjectRelativePath(absolute);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(StyleCollection.StandardSpace);
            if (GUILayout.Button("Set Build Scenes"))
            {
                int newScenesCount = AssetUtility.GetAssetsAtPath<SceneAsset>(sm.ScenePath).Length;
                if (EditorUtility.DisplayDialog("Set Build Scenes?",
                    $"By selecting 'Yes,' all scenes in the build will be replaced with the {newScenesCount} " +
                    $"{((newScenesCount != 1) ? "scenes" : "scene")} in '{sm.ScenePath}.' \n You may need to reorder scenes after committing this change.",
                    "Yes", "No"))
                {
                    SceneAdder.SetBuildScenes(sm.ScenePath);
                }
            }
        }
    }
}
