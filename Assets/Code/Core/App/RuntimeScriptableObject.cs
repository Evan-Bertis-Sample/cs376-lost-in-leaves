using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CurlyCore.CurlyApp
{
    /// <summary>
    /// A wrapper class for the common use pattern of using an IBooter ScriptableObject
    /// </summary>
    public abstract class RuntimeScriptableObject : ScriptableObject, IBooter, IQuiter
    {
        public virtual void OnBoot(App app, Scene scene) {}
        public virtual Task OnBootAsync(App app, Scene scene) { return Task.CompletedTask; }
        public virtual void OnQuit(App app, Scene scene) {}
    }
}