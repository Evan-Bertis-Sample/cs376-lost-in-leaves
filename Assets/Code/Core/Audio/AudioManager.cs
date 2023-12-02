using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;

using CurlyCore.CurlyApp;
using CurlyCore.Debugging;
using CurlyUtility;

namespace CurlyCore.Audio
{
    [CreateAssetMenu(menuName = "Curly/Core/Audio Manager", fileName = "AudioManager")]
    public class AudioManager : RuntimeScriptableObject
    {
        [System.Serializable]
        public class SerializableOverrideCache : SerializableDictionary<string, AudioOverride> { }

        [System.Serializable]
        public class SerializableGroupCache : SerializableDictionary<string, AudioGroup> { }

        [field: Header("Cascading Audio Configuration")]
        [field: SerializeField, DirectoryPath] public string AudioDirectoryRoot { get; private set; }
        [field: SerializeField] public AudioOverride DefaultGroupSettings { get; private set; }

        // Built in the inspector
        [field: Header("Cache -- Built in Editor")]
        [field: SerializeField] public SerializableOverrideCache OverrideCache { get; private set; }
        [field: SerializeField] public SerializableGroupCache GroupCache { get; private set; }

        [field: Header("Source Settings")]
        [field: SerializeField] public int StartingSourceCount = 10;
        [field: SerializeField] public float SourceGrowthRate = 1.5f;

        [field: Header("Background Music")]
        private AudioSource _backgroundSource;

        // Play mode bariables
        private GameObject _sourceParent;
        private List<AudioSource> _availableSources = new List<AudioSource>();
        private List<AudioSource> _unavailableSources = new List<AudioSource>();
        private Dictionary<AssetReference, AudioClip> _alreadyLoaded = new Dictionary<AssetReference, AudioClip>();
        private Dictionary<AudioClip, int> _usersPerClip = new Dictionary<AudioClip, int>();

        // Constants
        public readonly string AUDIO_GROUP_NAME = "Audio";
        public readonly string CLIP_LABEL = "Clips";
        public readonly string OVERRIDE_LABEL = "Override";
        public readonly string SOURCE_PARENT_NAME = "Audio Sources";
        public readonly string ONE_SHOT_SOURCE_OBJECT_NAME = "Audio Source";
        public readonly string BACKGROUND_SOURCE_OBJECT_NAME = "Background Source";

        [GlobalDefault] private GroupLogger _logger;

        public override void OnBoot(App app, Scene startingScene)
        {
            // Create audio sources
            _sourceParent = new GameObject(SOURCE_PARENT_NAME);
            // Add backgroundSource
            GameObject backgroundSourceObject = new GameObject(BACKGROUND_SOURCE_OBJECT_NAME);
            _backgroundSource = backgroundSourceObject.AddComponent<AudioSource>();
            backgroundSourceObject.transform.parent = _sourceParent.transform;

            // Configure oneshot sources
            _availableSources = new List<AudioSource>();
            _unavailableSources = new List<AudioSource>();
            GenerateOneShotSources(_sourceParent, StartingSourceCount);

            DontDestroyOnLoad(_sourceParent);

            DependencyInjector.InjectDependencies(this);
            _logger.Log(LoggingGroupID.APP, "Test Injection!");
        }

        public override void OnQuit(App app, Scene endingScene)
        {
            _availableSources.Clear();
            _unavailableSources.Clear();
        }

        public async void SetBackgroundMusicAsync(string musicPath, ITransition<AudioSource> inTransition = null, ITransition<AudioSource> outTransition = null, IAudioOverride iOverride = null)
        {
            string groupPath = IsFile(musicPath) ? Path.GetDirectoryName(musicPath).Replace("\\", "/") : musicPath;

            bool found = GroupCache.TryGetValue(groupPath, out AudioGroup group);

            if (found == false)
            {
                _logger.Log(LoggingGroupID.APP, $"Could not find Audio Group for path {group}", LogType.Warning);
                return;
            }

            AudioOverride groupOverride = group.Override;
            IAudioOverride appliedOverride = (iOverride == null) ? groupOverride : iOverride;
            AudioSource source = _backgroundSource;

            if (source.isPlaying) await inTransition?.Play(source);

            AssetReference clipReference = (groupOverride.IdentifyByFileName) ? group.ChooseClip(musicPath) : group.ChooseRandom();

            if (clipReference == null)
            {
                _logger.Log(LoggingGroupID.APP, $"Could not find Audio Reference for path {musicPath}", LogType.Warning);
                return;
            }

            AudioClip clip = await clipReference.LoadAssetAsync<AudioClip>().Task;

            source.clip = clip;
            appliedOverride.ApplyOverride(source);

            if (clip == null)
            {
                _logger.Log(LoggingGroupID.APP, $"Could not find Audio Clip for path {musicPath}", LogType.Warning);
                return;
            }

            _logger.Log(LoggingGroupID.APP, $"Found clip: {clip.name}");
            source.clip = clip;
            source.Play();

            await outTransition?.Play(source);
        }

        public void PlayOneShot(string soundPath, Vector3 position = default, IAudioOverride iOverride = null, Action<AudioCallback> OnPlay = null)
        {
            if (soundPath == null || soundPath == "") return;
            IEnumerator coroutine = PlayOneShotRoutine(soundPath, position, iOverride, OnPlay);
            App.Instance.CoroutineRunner.StartGlobalCoroutine(coroutine);
        }


        #region PlayOneShot Internals
        private IEnumerator PlayOneShotRoutine(string soundPath, Vector3 position = default, IAudioOverride iOverride = null, Action<AudioCallback> OnPlay = null)
        {
            Task<AudioCallback> callbackTask = PlayOneShotAsync(soundPath, position, iOverride);

            while (!callbackTask.IsCompleted) yield return null;

            AudioCallback result = callbackTask.Result;
            if (result != null)
                OnPlay?.Invoke(result);
        }

        private async Task<AudioCallback> PlayOneShotAsync(string soundPath, Vector3 position = default, IAudioOverride iOverride = null)
        {
            _logger.Log(LoggingGroupID.APP, $"Attempting to play one shot sound {soundPath}");

            string groupPath = IsFile(soundPath) ? Path.GetDirectoryName(soundPath).Replace("\\", "/") : soundPath;

            bool found = GroupCache.TryGetValue(groupPath, out AudioGroup group);

            if (found == false)
            {
                _logger.Log(LoggingGroupID.APP, $"Could not find Audio Group for path {groupPath}", LogType.Warning);
                return null;
            }

            AudioOverride groupOverride = group.Override;
            IAudioOverride appliedOverride = (iOverride == null) ? groupOverride : iOverride;
            AudioSource source = GetOneShotSource();
            _logger.Log(LoggingGroupID.APP, $"Found source: {source.gameObject.name}");
            appliedOverride.ApplyOverride(source);

            AssetReference clipReference = (groupOverride.IdentifyByFileName) ? group.ChooseClip(soundPath) : group.ChooseRandom();

            if (clipReference == null)
            {
                _logger.Log(LoggingGroupID.APP, $"Could not find Audio Reference for path {soundPath}", LogType.Warning);
                return null;
            }

            AudioClip clip;
            if (_alreadyLoaded.ContainsKey(clipReference)) clip = _alreadyLoaded[clipReference];
            else
            {
                try
                {
                    clip = await clipReference.LoadAssetAsync<AudioClip>().Task;
                    _alreadyLoaded[clipReference] = clip;
                }
                catch
                {
                    return null;
                }
            }

            if (_usersPerClip.ContainsKey(clip)) _usersPerClip[clip] = _usersPerClip[clip] + 1;
            else _usersPerClip[clip] = 1;

            if (clip == null)
            {
                _logger.Log(LoggingGroupID.APP, $"Could not find Audio Clip for path {soundPath}", LogType.Warning);
                return null;
            }

            _logger.Log(LoggingGroupID.APP, $"Found clip: {clip.name}");
            source.clip = clip;
            source.Play();
            AudioCallback callback = new AudioCallback(source, this);
            callback.OnAudioEnd += RestashSource;
            callback.OnAudioEnd += source =>
            {
                _usersPerClip[clip] = _usersPerClip[clip] - 1;

                if (_usersPerClip[clip] <= 0)
                {
                    // it's time to free this resource
                    Addressables.Release(clip);
                    _alreadyLoaded.Remove(clipReference);
                }
            };
            return callback;
        }
        #endregion

        public AudioOverride GetOverride(string soundPath) => OverrideCache[soundPath];

        public AudioGroup GetGroup(string soundPath) => GroupCache[soundPath];

#if UNITY_EDITOR
        public void SetOverrideCache(Dictionary<string, AudioOverride> cache)
        {
            OverrideCache = new SerializableOverrideCache();
            foreach (var pair in cache)
            {
                OverrideCache[pair.Key] = pair.Value;
            }
        }

        public void SetGroupCache(Dictionary<string, AudioGroup> cache)
        {
            GroupCache = new SerializableGroupCache();
            foreach (var pair in cache)
            {
                GroupCache[pair.Key] = pair.Value;
            }
        }
#endif

        private bool IsFile(string path)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(path);

                //detect whether its a directory or file  
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    return false;
                else
                    return true;
            }
            catch
            {
                _logger.Log(LoggingGroupID.APP, $"Could not find file at path {path}", LogType.Warning);
                return false;
            }
        }

        private void GenerateOneShotSources(GameObject parent, int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject sourceObject = new GameObject($"{ONE_SHOT_SOURCE_OBJECT_NAME} ({i})");
                AudioSource source = sourceObject.AddComponent<AudioSource>();
                sourceObject.SetActive(false);
                sourceObject.transform.parent = parent.transform;
                _availableSources.Add(source);
            }
        }

        private AudioSource GetOneShotSource()
        {
            if (_availableSources.Count == 0)
            {
                int count = Mathf.FloorToInt(_unavailableSources.Count * SourceGrowthRate - _unavailableSources.Count);

                GenerateOneShotSources(_sourceParent, count);
            }

            AudioSource source = _availableSources[0];
            _availableSources.RemoveAt(0);
            _unavailableSources.Add(source);
            source.gameObject.SetActive(true);

            return source;
        }

        public void RestashSource(AudioSource source)
        {
            _unavailableSources.Remove(source);
            source.gameObject.SetActive(false);
            _availableSources.Add(source);
        }
    }
}