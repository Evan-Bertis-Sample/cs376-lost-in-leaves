using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurlyUtility;
using UnityEngine;

namespace CurlyCore.Audio.Transitions
{
    public abstract class AudioTransitionObject : ScriptableObject, ITransition<AudioSource>
    {
        public abstract void EndTransition(AudioSource value);
        public abstract void PrepareTransition(AudioSource value);
        public abstract Task Transition(AudioSource value);
    }
}
