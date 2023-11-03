using UnityEngine;
using UnityEditor;
using CurlyCore.CurlyApp;

namespace CurlyEditor.Core.CurlyApp
{
    public class AppConfigEditor : EditorWindow
    {
        private AppConfig _appConfig;
        private UnityEditor.Editor _configEditor;

        [MenuItem("Curly/Core/App")]
        public static void ShowWindow()
        {
            GetWindow<AppConfigEditor>("App Configuration");
        }

        private void OnEnable()
        {
            _appConfig = Resources.Load<AppConfig>(App.ConfigPath);
        }

        private void OnGUI()
        {
            if (_appConfig == null)
            {
                EditorGUILayout.HelpBox($"AppConfig not found in Resources/{App.ConfigPath}.", MessageType.Error);
            }
            else
            {
                UnityEditor.Editor.CreateCachedEditor(_appConfig, null, ref _configEditor);
                _configEditor.OnInspectorGUI();
            }
        }
    }
}