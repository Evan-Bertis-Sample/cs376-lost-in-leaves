using System.Collections;
using System.Collections.Generic;
using LostInLeaves.Interactables;
using UnityEngine;
using CurlyCore.Audio;
using CurlyCore;

namespace LostInLeaves
{
    public class SoundInteractable : InteractableObject
    {
        [Header("Sound Settings")]
        [SerializeField, AudioPath] private string _audioPath;
        [SerializeField] private AudioOverride _audioOverride;
        [SerializeField] private Transform _audioTarget;

        [GlobalDefault] private AudioManager _audioManager;
        private bool _isPlaying = false;

        private Vector3 _targetPosition => _audioTarget != null ? _audioTarget.position : transform.position;

        protected override void OnInteractStart(Interactor interactor)
        {
            if (_isPlaying) return;
            if (_audioManager == null) DependencyInjector.InjectDependencies(this);

            _isPlaying = true;
            _audioManager.PlayOneShot(_audioPath, _targetPosition, _audioOverride, callback =>
            {
                callback.OnAudioEnd += source => _isPlaying = false;
            });
        }
    }
}
