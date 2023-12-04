using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using CurlyCore.SceneManagement;
using CurlyCore.Input;
using CurlyCore.Debugging;
using CurlyCore.Audio;

namespace CurlyCore.CurlyApp
{
    /// <summary>
    /// Configuration Class for App. An instance must be found under Resources for App to work
    /// </summary>
    [CreateAssetMenu(menuName = "Curly/Core/App Configuration", fileName = "AppConfig")]
    public class AppConfig : ScriptableObject
    {
        [field: Header("Booter Configuration")]
        [field: SerializeField] public List<RuntimeScriptableObject> Booters {get; private set;} = new List<RuntimeScriptableObject>();

        [field: Header("Core Configuration")]
        [field: SerializeField] public SceneMaster SceneMaster {get; private set;}
        [field: SerializeField] public InputManager InputManager {get; private set;}
        [field: SerializeField] public GroupLogger Logger {get; private set;}
        [field: SerializeField] public AudioManager AudioManger {get; private set;}
        [field: SerializeField] public bool EnableDebugging {get; private set;} = false;

        [field: Header("Global Defaults")]
        [field: SerializeField] public List<ScriptableObject> GlobalDefaultSystems {get; private set;}
        
        [Header("Gameplay")]
        // All objects that should be placed in the DontDestroyOnLoad scene on gamestart
        [SerializeField] public List<GameObject> PersistentObjects = new List<GameObject>();
    }
}
