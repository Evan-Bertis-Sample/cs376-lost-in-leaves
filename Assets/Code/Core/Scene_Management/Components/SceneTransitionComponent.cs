using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CurlyCore.CurlyApp;

namespace CurlyCore.SceneManagement.Components
{
    public class SceneTransitionComponent : MonoBehaviour
    {
        public string TargetScene;
        public bool UseDefaultTransitions = true;

        [GlobalDefault] private SceneMaster _sceneMaster;

        public void TransitionScene()
        {
            _sceneMaster.LoadSceneAsync(TargetScene, UseDefaultTransitions);
        }
    }
}
