using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurlyCore.Audio
{
    [CreateAssetMenu(fileName = "AudioOverride", menuName = "Curly/Audio/Audio Override"), System.Serializable]
    public class AudioOverride : ScriptableObject, IAudioOverride
    {
        [System.Serializable]
        public class Range
        {
            public float Min = 0.5f;
            public float Max = 0.75f;
        }

        [field: SerializeField] public bool IdentifyByFileName {get; private set;} // If this flag is true, then we consider each sound file in the directory as a different sfx -- they are identified by name, not directory
        [field: SerializeField] public bool UseRandomPitch {get; private set;}
        [field: SerializeField] public bool UseRandomVolume {get; private set;}
        [field: SerializeField] public float FlatVolumeMultiplier {get; private set;} = 1f;
        [field: SerializeField] public float FlatPitchMultiplier {get; private set;} = 1f;
        [field: SerializeField] public Range PitchMultiplerRange {get; private set;}
        [field: SerializeField] public Range VolumeMultiplierRange {get; private set;}
        [field: SerializeField] public bool Loop {get; private set;} = false;

        public void ApplyOverride(AudioSource source)
        {
            if (UseRandomPitch) source.pitch = GetRandomInRange(PitchMultiplerRange) * FlatPitchMultiplier;
            else source.pitch = FlatPitchMultiplier;
            if (UseRandomVolume) source.volume = GetRandomInRange(VolumeMultiplierRange) * FlatVolumeMultiplier;
            else source.volume = FlatVolumeMultiplier;
            source.loop = Loop;
        }

        private float GetRandomInRange(Range range)
        {
            return Random.Range(range.Min, range.Max);
        }
    }
}