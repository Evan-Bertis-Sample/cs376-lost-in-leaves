using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using UnityEngine;
using UnityEditor;

using CurlyCore.Input;
using CurlyCore.CurlyApp;
using System;

using Object = UnityEngine.Object;
using CurlyEditor.Utility;
using CurlyCore;

#if UNITY_EDITOR
namespace CurlyEditor.Core
{
    public class InputPathValidator
    {
        // Use a dictionary to hold invalid values and the fields that hold them
        private static Dictionary<string, List<(Object obj, FieldInfo field)>> invalidValues = new Dictionary<string, List<(Object obj, FieldInfo field)>>();

        [MenuItem("Curly/Tools/Validate InputPaths")]
        public static void ValidateInputPaths()
        {
            invalidValues.Clear();

            // Prompt the user to select the InputManager
            InputManagerSelectionWindow.ShowWindow(SelectedInputManagerCallback);
        }

        private static void SelectedInputManagerCallback(InputManager manager)
        {
            if (manager == null)
            {
                Debug.Log("Validating Inputs using Default Manager...");
                manager = GlobalDefaultStorage.GetDefault(typeof(InputManager)) as InputManager;
            }

            invalidValues.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Object");  // This finds all assets

            InputManager defaultManager = GlobalDefaultStorage.GetDefault(typeof(InputManager)) as InputManager;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj == null) continue;

                InputManager objectInputManager = ReflectionUtility.GetFieldByType<InputManager>(obj);

                if (objectInputManager == null)
                {
                    // This is using the default
                    objectInputManager = defaultManager;
                }

                if (objectInputManager != manager) continue; // We don't need to validate this object

                var type = obj.GetType();
                FieldInfo[] allFields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (FieldInfo field in allFields)
                {
                    if (InputPathAttribute.IsDefined(field, typeof(InputPathAttribute)))
                    {
                        bool isValid = ValidateInputPath(obj, field, manager);
                        if (!isValid)
                        {
                            string invalidValue = (string)field.GetValue(obj);
                            Debug.LogError($"Invalid InputPath at {path}, field {field.Name}, value: {invalidValue}");

                            if (!invalidValues.ContainsKey(invalidValue))
                            {
                                invalidValues[invalidValue] = new List<(Object obj, FieldInfo field)>();
                            }

                            invalidValues[invalidValue].Add((obj, field));
                        }
                    }
                }
            }

            if (invalidValues.Count == 0)
            {
                Debug.Log("InputPaths validated!");
            }

            foreach (string invalidValue in invalidValues.Keys)
            {
                InputPathCorrectionWindow.Init(invalidValue, invalidValues[invalidValue]);
            }
        }

        private static bool ValidateInputPath(Object obj, FieldInfo field, InputManager manager)
        {
            string fieldValue = (string)field.GetValue(obj);
            return manager.IsInputAssigned(fieldValue);
        }
    }

}

public class InputManagerSelectionWindow : EditorWindow
{
    private InputManager selectedInputManager;

    public static void ShowWindow(Action<InputManager> callback)
    {
        InputManagerSelectionWindow window = GetWindow<InputManagerSelectionWindow>("Select Input Manager");
        window.callback = callback;
    }

    private Action<InputManager> callback;

    void OnGUI()
    {
        EditorGUILayout.LabelField("Please select the InputManager to use for validation:");

        selectedInputManager = (InputManager)EditorGUILayout.ObjectField("Input Manager", selectedInputManager, typeof(InputManager), false);

        if (GUILayout.Button("Validate"))
        {
            callback?.Invoke(selectedInputManager);
            Close();
        }
    }
}

#endif