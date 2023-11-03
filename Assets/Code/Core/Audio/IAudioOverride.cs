using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurlyCore.Audio
{
    public interface IAudioOverride
    {
        public void ApplyOverride(AudioSource source);
    }
}