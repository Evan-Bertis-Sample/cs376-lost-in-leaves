using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using CurlyCore.CurlyApp;
using CurlyCore.Input;

namespace CurlyEditor.Core
{
    public class InputPathCorrectionWindow : EditorWindow
    {
        private class _InputPathWrapper : ScriptableObject
        {
            [InputPath] public string InputPath;
        }

        private string _invalidValue;
        private List<(Object obj, FieldInfo field)> _fieldsToCorrect;
        private string _newValue;

        private Editor _editor;
        private _InputPathWrapper _wrapper;

        public static void Init(string invalidValue, List<(Object obj, FieldInfo field)> fieldsToCorrect)
        {
            InputPathCorrectionWindow window = ScriptableObject.CreateInstance<InputPathCorrectionWindow>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            window.ShowUtility();
            window.Initialize(invalidValue, fieldsToCorrect);
        }

        public void Initialize(string invalidValue, List<(Object obj, FieldInfo field)> fieldsToCorrect)
        {
            _invalidValue = invalidValue;
            _fieldsToCorrect = fieldsToCorrect;
            _newValue = _invalidValue;
            _wrapper = ScriptableObject.CreateInstance<_InputPathWrapper>();
            _wrapper.InputPath = _newValue;
            Focus();
        }

        void OnGUI()
        {
            GUILayout.Label("Invalid Input Path", EditorStyles.boldLabel);
            GUILayout.Label($"Invalid Value: {_invalidValue}");

            // Ghetto way of getting arround this
            Editor.CreateCachedEditor(_wrapper, null, ref _editor);
            _editor.OnInspectorGUI();
            _newValue = _wrapper.InputPath;

            if (GUILayout.Button($"Correct Input Path to '{_newValue}'"))
            {
                foreach (var (obj, field) in _fieldsToCorrect)
                {
                    Debug.Log($"Updating {field.Name} to value {_newValue}");
                    Undo.RecordObject(obj, "Correct Input Path");
                    field.SetValue(obj, _newValue);
                    EditorUtility.SetDirty(obj); // This makes sure the changes are saved
                }
                
                AssetDatabase.SaveAssets();
                this.Close();
            }
        }
    }
}
