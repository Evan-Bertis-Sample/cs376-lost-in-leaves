using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

using CurlyUtility;

namespace CurlyCore.SceneManagement
{
    public abstract class ScreenTransitionObject : ScriptableObject, ITransition<Canvas>
    {
        public abstract void EndTransition(Canvas screenCanvas);
        public abstract void PrepareTransition(Canvas screenCanvas);
        public abstract Task Transition(Canvas screenCanvas);
    }
}

