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
        [System.Serializable]
        internal struct Range
        {
            public float Min;
            public float Max;
        }

        [Header("Sound Settings")]
        [SerializeField, AudioPath] private string _audioPath;
        [SerializeField] private AudioOverride _audioOverride;
        [SerializeField] private Transform _audioTarget;

        [Header("Randomization Settings")]
        [SerializeField] private Range _entranceDelayRange;
        [SerializeField] private bool _repeat;
        [SerializeField] private Range _repeatDelayRange;

        [GlobalDefault] private AudioManager _audioManager;
        private bool _isPlaying = false;

        private Vector3 _targetPosition => _audioTarget != null ? _audioTarget.position : transform.position;

        protected override void OnInteractStart(Interactor interactor)
        {
            if (_isPlaying) return;
            if (_audioManager == null) DependencyInjector.InjectDependencies(this);
            StartCoroutine(PlaySoundInitial());
        }

        protected override void Interact(Interactor interactor)
        {
            if (!_repeat) return;
            if (_isPlaying) return;
            StartCoroutine(PlaySoundRepeat());
        }

        private IEnumerator PlaySoundInitial()
        {
            _isPlaying = true;
            yield return new WaitForSeconds(Random.Range(_entranceDelayRange.Min, _entranceDelayRange.Max));
            _audioManager.PlayOneShot(_audioPath, _targetPosition, _audioOverride, callback =>
            {
                callback.OnAudioEnd += source => _isPlaying = false;
            });
        }

        private IEnumerator PlaySoundRepeat()
        {
            _isPlaying = true;
            yield return new WaitForSeconds(Random.Range(_repeatDelayRange.Min, _repeatDelayRange.Max));
            _audioManager.PlayOneShot(_audioPath, _targetPosition, _audioOverride, callback =>
            {
                callback.OnAudioEnd += source => _isPlaying = false;
            });
        }
    }
}
