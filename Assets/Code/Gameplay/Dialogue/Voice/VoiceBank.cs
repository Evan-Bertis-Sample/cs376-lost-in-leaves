using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CurlyCore.Audio;
using CurlyUtility;
using CurlyCore;

namespace LostInLeaves.Dialogue
{
    [CreateAssetMenu(menuName = "Lost In Leaves/Dialogue/Voice Bank", fileName = "voice-bank")]
    public class VoiceBank : ScriptableObject
    {
        [System.Serializable]
        internal class VoiceSettings
        {
            public string CharacterName;
            [DirectoryPath] public string Folder;
            public AudioOverride Override;
        }

        [SerializeField] private VoiceSettings _defaultSettings;
        [SerializeField] private bool _resortToDefault = true;
        [SerializeField] private List<VoiceSettings> _voiceSettings = new List<VoiceSettings>();

        [GlobalDefault] private AudioManager _audioManager;
        private Dictionary<string, VoiceSettings> _voiceSettingsDict;

        public void PlayVoice(string characterName, char c, Vector3 position = default)
        {
            if (_audioManager == null) 
            {
                DependencyInjector.InjectDependencies(this);
            }

            if (_voiceSettingsDict == null)
            {
                _voiceSettingsDict = new Dictionary<string, VoiceSettings>();
                foreach (var s in _voiceSettings)
                {
                    _voiceSettingsDict[s.CharacterName] = s;
                }
            }

            bool found = _voiceSettingsDict.TryGetValue(characterName, out var settings);

            if (!found && _resortToDefault)
            {
                settings = _defaultSettings;
            }

            if (settings == null)
            {
                Debug.LogWarning($"VoiceBank: No voice settings found for character {characterName}, and not resorting to default.");
                return;
            }

            string path = BuildVoiceCharPath(settings.Folder, c);

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"VoiceBank: No voice file found for character {c} in folder {settings.Folder}.");
                return;
            }

            _audioManager.PlayOneShot(path, position, settings.Override);
        }

        public string BuildVoiceCharPath(string voiceFolder, char c)
        {
            if (char.IsLetter(c) == false) return "";

            // Get the character name from the folder name
            string characterName = voiceFolder.Substring(voiceFolder.LastIndexOf('/') + 1);
            char upper = char.ToUpper(c);

            return $"{voiceFolder}/{characterName}_{upper}.mp3";
        }
    }
}
