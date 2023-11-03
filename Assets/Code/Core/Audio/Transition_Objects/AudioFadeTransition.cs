using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CurlyUtility;
using DG.Tweening;

namespace CurlyCore.Audio.Transitions
{
    [CreateAssetMenu(menuName = "Curly/Audio/Transitions/Audio Fade")]
    public class AudioFadeTransition : AudioTransitionObject
    {
        public float Duration = 1f; // The duration of the transition
        public float TargetVolume = 1f; // The target volume at the end of the transition

        public override void EndTransition(AudioSource value)
        {
            value.volume = TargetVolume;
        }

        public override void PrepareTransition(AudioSource value)
        {
            // No need to set the initial volume here, as the transition will start from the current volume
        }

        public override async Task Transition(AudioSource value)
        {
            await value.DOFade(TargetVolume, Duration).AsyncWaitForCompletion(); // Fade from current volume to target volume
        }
    }
}