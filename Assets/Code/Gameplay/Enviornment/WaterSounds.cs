using System.Collections;
using System.Collections.Generic;
using CurlyCore.Audio;
using CurlyCore;
using UnityEngine;

namespace LostInLeaves.Enviornment
{
    /// <summary>
    /// A class that is responsible for playing water sounds depending on the player's position.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class WaterSounds : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _maxDistance = 10f;
        [SerializeField] private AudioOverride _waterOverride;
        [SerializeField] private float _falloffSpeed = 0.1f;
        [SerializeField] private Terrain _terrain;
        [SerializeField, AudioPath] private string _waterSoundPath;

        [Header("Water Detection Settings")]
        [SerializeField] private int _numRays = 16;
        [SerializeField] private float _rayRadius = 1f;
        [SerializeField] private int _samplesPerRay = 10;
        
        [GlobalDefault] private AudioManager _audioManager;

        private Renderer _renderer;
        private AudioCallback _callback;
        private float _waterLevel;
        private bool _isPlaying = false;
        private float _waterVolume = 0;
        private const float _minVolume = 0.001f;

        private void Start()
        {
            DependencyInjector.InjectDependencies(this);
            _waterLevel = transform.position.y;
            _renderer = GetComponent<Renderer>();
            _isPlaying = false;
        }

        private void Update()
        {
            CalculateVolume();

            if (_waterVolume > _minVolume && !_isPlaying)
            {
                // play the sound
                // we have to set it to true here because the callback is not set until after the sound is played
                // that could occur after the next frame, so we need to set it to true here
                Debug.Log("Starting sound");
                _isPlaying = true; 
                _audioManager.PlayOneShot(_waterSoundPath, _playerTransform.position, _waterOverride, callback =>
                {
                    _callback = callback;
                    _callback.Source.volume = _waterVolume;
                });
            }

            // Handle the volume of the sound
            if (_isPlaying && _waterVolume <= _minVolume)
            {
                Debug.Log("Stopping sound");
                _callback.ForceStop();
                _callback = null;
                _isPlaying = false;
            }
            else if (_isPlaying)
            {
                _callback.Source.volume = _waterVolume;
            }
        }

        private void CalculateVolume()
        {
            float distance = _playerTransform.position.y - _waterLevel;

            // calcluate the volume based on the distance from the player, such that it gets exponentially 
            float waterBaseVolume = _waterOverride.FlatVolumeMultiplier;
            // use the inverse square law to calculate the volume
            float t = Mathf.Clamp01(distance / _maxDistance);
            float volume = Mathf.Lerp(waterBaseVolume, 0, Mathf.Pow(t, 2));
            // take into account whether or not the water is visible, if it isn't, we should lerp the volume to 0
            // else, we should lerp it to the calculated volume
            if (IsVisible())
            {
                _waterVolume = Mathf.Lerp(_waterVolume, volume, _falloffSpeed * Time.deltaTime);
            }
            else
            {
                Debug.Log("Lerping to 0");
                _waterVolume = Mathf.Lerp(_waterVolume, 0, _falloffSpeed * Time.deltaTime);
            }

        }

        private bool IsVisible()
        {
            // sample the terrain from multiple positions around the player to see if the water is visible
            // we do this because the water is not always visible from the player's position
            for(int i = 0; i < _numRays; i++)
            {
                float theta = (i / (float)_numRays) * Mathf.PI * 2;
                Vector3 offset = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * _rayRadius;

                for(int j = 1; j <= _samplesPerRay; j++) // start at 1 because we don't want to sample the player's position
                {
                    float t = j / ((float)_samplesPerRay);
                    Vector3 sampleOffset = offset * t;
                    float terrainHeight = GetTerrainHeight(sampleOffset);
                    if (terrainHeight < _waterLevel)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private float GetTerrainHeight(Vector3 offsetFromPlayer)
        {
            // get the height of the terrain at the water's position
            float terrainHeight = _terrain.SampleHeight(_playerTransform.position + offsetFromPlayer);

            // now we need to map the terrain height to world space
            // we can do this by getting the terrain's position and adding the height to it
            terrainHeight += _terrain.transform.position.y;

            return terrainHeight;
        }
    }
}
