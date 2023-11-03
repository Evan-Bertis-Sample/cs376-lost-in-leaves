using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CurlyCore.CurlyApp;
using CurlyUtility;

namespace CurlyCore.SceneManagement
{
    [CreateAssetMenu(menuName = "Curly/Core/Scene Master", fileName = "SceneMaster")]
    public sealed class SceneMaster : RuntimeScriptableObject
    {
        // Configuration
        [field: SerializeField] public string ScenePath;

        [field: Header("Scene Transitions")]
        [field: SerializeField] public ScreenTransitionObject DefaultInTransition;
        [field: SerializeField] public ScreenTransitionObject DefaultOutTransition;

        // Events
        public delegate void SceneHandler(string oldScene, string newScene);

        // * CALL ORDER
        // * OnSceneUnloadAnim -> OnSceneUnload -> OnSceneLoad -> OnSceneLoadAnim

        // Occurs the moment that a scene is unloaded, disregarding animations
        public SceneHandler OnSceneUnload;
        // Occurs the moment that a scene is loading, disregarding animations
        public SceneHandler OnSceneLoad;
        // Occurs the moment that a scene begins to get unloaded, as the transition animation begins
        public SceneHandler OnSceneUnloadAnim;
        // Occurs the moment that a scene finishes loading, after the transition player plays
        public SceneHandler OnSceneLoadAnim;

        public override void OnBoot(App app, Scene scene)
        {
            CreateScreenCanvas();
        }

        public async void LoadSceneAsync(string sceneName, bool useDefault)
        {
            if (useDefault) LoadSceneAsync(sceneName, DefaultInTransition, DefaultOutTransition);
            else LoadSceneAsync(sceneName, null, null);
        }

        /// <summary>
        /// Loads a scene async using the transition in and transition out animations
        /// </summary>
        /// <param name="sceneName"> The desired scene to transition into </param>
        /// <param name="transitionIn"> The transition played at the beginning of the transition </param>
        /// <param name="transitionOut"> The transition played leaving the transition </param>
        /// <returns></returns>
        public async void LoadSceneAsync(string sceneName, ITransition<Canvas> transitionIn = null, ITransition<Canvas> transitionOut = null)
        {
            Scene curScene = SceneManager.GetActiveScene();
            Debug.Log($"Loading scene {sceneName} from {curScene.name}");


            AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            loadingOperation.allowSceneActivation = false;

            // ENTER LOAD
            List<Task> loadingTasks = new List<Task>();
            // Add actual loading task
            loadingTasks.Add(TaskUtility.WaitUntil(() =>
            {
                Debug.Log($"Loading... {loadingOperation.progress * 100}%");
                return loadingOperation.progress >= .9f;
            }));

            // Play transitionIn animation
            Canvas screenCanvas = CreateScreenCanvas();
            DontDestroyOnLoad(screenCanvas);
            if (transitionIn != null) loadingTasks.Add(transitionIn.Play(screenCanvas));
            // Event
            OnSceneUnloadAnim?.Invoke(curScene.name, sceneName);
            // Wait for both to finish
            await Task.WhenAll(loadingTasks);

            // Now the scene is basically done loading and the animation has played, now allow for scene change
            loadingOperation.allowSceneActivation = true;

            // Now wait for the scene to load
            await TaskUtility.WaitUntil(() =>
            {
                Debug.Log($"Loading... {loadingOperation.progress * 100}%");
                return loadingOperation.isDone;
            });

            OnSceneUnload?.Invoke(curScene.name, sceneName);
            SceneManager.UnloadSceneAsync(curScene.name);
            OnSceneLoad?.Invoke(curScene.name, sceneName);

            // FINISH LOAD
            if (transitionOut != null) await transitionOut.Play(screenCanvas);
            OnSceneLoadAnim?.Invoke(curScene.name, sceneName);

            // Destroy the canvas
            Destroy(screenCanvas.transform.parent);
        }

        private Canvas CreateScreenCanvas()
        {
            // Create a new GameObject in the scene
            GameObject canvasObject = new GameObject("Transition Canvas");

            // Add a Canvas component to the GameObject
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Ensure that the Canvas is on top of all other UI elements
            canvas.sortingOrder = short.MaxValue;

            // Add a CanvasScaler component to ensure the Canvas covers the full screen
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            // Add a GraphicRaycaster component to enable input events
            canvasObject.AddComponent<GraphicRaycaster>();

            // Create a RectTransform and set it to cover the whole screen
            RectTransform rect = canvas.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0.5f, 0.5f);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.zero;

            return canvas;
        }
    }
}
