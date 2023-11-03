using UnityEngine;

namespace CurlyCore.CurlyApp
{
    /// <summary>
    // Bootstrap Class
    /// </summary>
    public sealed class Bootstrap
    {
        /// <summary>
        /// Initialization method that runs before any scene has loaded
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static async void Initialize()
        {
            await App.Instance.InitializeGame();
        }
    }
}