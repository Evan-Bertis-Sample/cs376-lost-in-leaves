using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CurlyUtility;
using CurlyCore.CurlyApp;
using CurlyCore.Audio;
using CurlyCore;
using CurlyCore.Debugging;

public class PlayClipOnStart : MonoBehaviour
{
    [AudioPath] public string AudioPath;

    [GlobalDefault] private AudioManager _audioManager;
    [GlobalDefault] private GroupLogger _logger;

    private void Start()
    {
        DependencyInjector.InjectDependencies(this);
        _audioManager.PlayOneShot(AudioPath, default, null,
        callback =>
        {
            callback.OnAudioStart += source => Log(source, "AUDIO START");
            callback.OnAudioEnd += source => Log(source, "AUDIO END");
        });
    }

    private void Log(AudioSource source, string message)
    {
        _logger.Log(CurlyCore.Debugging.LoggingGroupID.APP, message);
    }
}
