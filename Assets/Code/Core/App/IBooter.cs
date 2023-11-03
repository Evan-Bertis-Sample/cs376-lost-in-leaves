using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CurlyCore.CurlyApp
{
    public interface IBooter
    {
        /// <summary>
        /// Called on game startup, given that that the AppConfig knows of it's existence
        /// </summary>
        /// <param name="app"> The App object calling the Boostrap </param>
        /// <param name="scene"> The Scene that the game is starting in</param>
        public void OnBoot(App app, Scene scene);

        public Task OnBootAsync(App app, Scene scene);
    }

    public interface IQuiter
    {
        /// <summary>
        /// Called on game end, given that the AppConfig knows of it's existence
        /// </summary>
        /// <param name="app"> The App object calling the App Quit</param>
        /// <param name="scene"> The Scene that the game is quit in</param>
        public void OnQuit(App app, Scene scene);
    }

}