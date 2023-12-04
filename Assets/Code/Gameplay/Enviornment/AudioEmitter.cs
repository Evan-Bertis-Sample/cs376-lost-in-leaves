using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CurlyCore.Audio;
using CurlyCore;
using LostInLeaves.Player;

namespace LostInLeaves.Components
{
    public class AudioEmitter : MonoBehaviour
    {
        [SerializeField, AudioPath] private string _audioPath;
        [SerializeField] private float _maxDistance = 10f;
        [SerializeField] private AudioOverride _audioOverride;
        [SerializeField] private float _volumeMultiplier = 0.5f;
        [GlobalDefault] private AudioManager _audioManager;

        private Vector3 PlayerPosition => PlayerController.Instance.transform.position;
        private float MaxVolume => (_audioOverride != null ? _audioOverride.FlatVolumeMultiplier : 1f) * _volumeMultiplier;
        private bool _isPlaying;

        private AudioCallback _audioCallback;
        private void Awake()
        {
            DependencyInjector.InjectDependencies(this);
        }

        private void Update()
        {
            float distance = Vector3.Distance(PlayerPosition, transform.position);

            if (distance > _maxDistance)
            {
                if (_audioCallback != null && _isPlaying)
                {
                    _audioCallback.ForceStop();
                    _audioCallback = null;
                    _isPlaying = false;
                }
                return;
            }

            if (_audioCallback == null && !_isPlaying)
            {
                _isPlaying = true;
                _audioManager.PlayOneShot(_audioPath, transform.position, _audioOverride, callback =>
                {
                    _audioCallback = callback;
                    _audioCallback.Source.volume = Mathf.Lerp(0, MaxVolume, 1 - (distance / _maxDistance));
                });
            }
            else if (_audioCallback != null && _isPlaying)
            {
                _audioCallback.Source.volume = Mathf.Lerp(0, MaxVolume, 1 - (distance / _maxDistance));
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _maxDistance);
            Gizmos.color = Color.white;
        }
    }
}

