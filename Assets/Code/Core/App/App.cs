using System.Linq;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

using CurlyCore.SceneManagement;
using CurlyCore.Input;
using CurlyCore.Debugging;
using CurlyCore.Audio;
using System;

// TODO: Make AppConfig chooseable in Editor
namespace CurlyCore.CurlyApp
{
    /// <summary>
    /// A singleton devoted to initalizing the game no matter what scene is the first scene
    /// </summary>
    public sealed class App
    {
        #region Singleton setup
        private static App _instance;
        public static App Instance
        {
            get
            {
                if (_instance == null) _instance = new App();
                return _instance;
            }
        }
        #endregion

        private readonly AppConfig _config;

        // Fields
        public static string ConfigPath = "Core_Config/AppConfig";
        public AppConfig Config => _config;

        // Coroutine stuff
        public CoroutineRunner CoroutineRunner { get; private set; }


        private App()
        {
            _config = Resources.Load<AppConfig>(App.ConfigPath);
        }

        /// <summary>
        /// Adds all dependencies on gamestartup and performs level loading logic if the starting scene is a level
        /// </summary>
        public Task InitializeGame()
        {
            if (_config == null)
            {
                Debug.LogError("Could not find App Config!");
                return Task.CompletedTask;
            }

            SpawnCoroutineMaster();
            RegisterDefaults();
            HandleBooters();
            DressScene();

            return Task.CompletedTask;
        }

        public void RegisterDefaults()
        {
            // Debug.Log("Registering defaults...");
            foreach (ScriptableObject so in _config.GlobalDefaultSystems)
            {
                Type soType = so.GetType();
                GlobalDefaultStorage.RegisterDefault(soType, so);
            }
        }

        private async void HandleBooters()
        {
            Scene startingScene = SceneManager.GetActiveScene();
            // Instantiate Quitter
            GameObject quit = new GameObject("Quitter");
            GameObject.DontDestroyOnLoad(quit.transform.root.gameObject);

            Quiter quitterComponent = quit.AddComponent<Quiter>();

            foreach (RuntimeScriptableObject b in _config.Booters)
            {
                if (b == null) continue;
                b.OnBoot(this, startingScene);
                quitterComponent.OnQuit += () => b.OnQuit(this, startingScene);
                await b.OnBootAsync(this, startingScene);
            }

        }

        /// <summary>
        /// Adds all the persistent gameobjects to the scene upon game load. These objects are added to the DDOL Scene
        /// </summary>
        private void DressScene()
        {
            if (_config.PersistentObjects == null) return;

            foreach (GameObject obj in _config.PersistentObjects)
            {
                // Instantiate and put into DDOL scene
                if (obj == null) continue;
                GameObject instance = GameObject.Instantiate(obj, Vector3.zero, Quaternion.identity);
                GameObject.DontDestroyOnLoad(instance.transform.root.gameObject);
            }
        }

        private void SpawnCoroutineMaster()
        {
            GameObject instance = new GameObject("Coroutine Master");
            CoroutineRunner = instance.AddComponent<CoroutineRunner>();
            GameObject.DontDestroyOnLoad(instance);
        }
    }
}
