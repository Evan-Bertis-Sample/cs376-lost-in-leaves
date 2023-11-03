using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

using CurlyCore.Audio;
using CurlyEditor.Utility;
using CurlyUtility;

namespace CurlyEditor.Core
{
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AudioManager manager = target as AudioManager;
            if (manager.AudioDirectoryRoot != "" && GUILayout.Button("Build Audio"))
            {
                BuildAudio(manager);
                EditorUtility.SetDirty(manager);
            }
        }

        private void BuildAudio(AudioManager manager)
        {
            string audioPath = manager.AudioDirectoryRoot;
            Debug.Log($"Building Addressables at {audioPath}");

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            // Find or create the "Audio" group
            AddressableAssetGroup audioGroup = settings.FindGroup(manager.AUDIO_GROUP_NAME);
            if (audioGroup == null)
            {
                audioGroup = settings.CreateGroup(manager.AUDIO_GROUP_NAME, false, false, true, new List<AddressableAssetGroupSchema>());
            }
            // Process the root directory
            ProcessAddressables(manager, audioPath, settings, audioGroup);

            CreateOverrideCache(manager);
            CreateGroupCache(manager, settings);

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true, true);

            // Clear progress bar when build is done
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Proccess the Audio directory
        /// </summary>
        /// <param name="manager"> The AudioManager that dictates how the Directory is processed </param>
        /// <param name="directoryPath"> The path of the directory </param>
        /// <param name="settings"> Addressables settings </param>
        /// <param name="group"> The Addressables group to place the processed entries </param>
        private void ProcessAddressables(AudioManager manager, string directoryPath, AddressableAssetSettings settings, AddressableAssetGroup group)
        {
            // Get the GUIDs of the directory itself, any AudioOverrideGroups and any AudioClips in this directory
            string[] childDirectories = Directory.GetDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly);

            // Now recursively process any subdirectories
            string parentGuid = AssetDatabase.AssetPathToGUID(directoryPath);
            AddressableAssetEntry parentEntry = settings.FindAssetEntry(parentGuid);
            if (parentEntry != null && childDirectories.Length > 0)
            {
                parentEntry.IsFolder = true;
            }

            // Process each GUID
            foreach (string childDirectory in childDirectories)
            {
                string directory = childDirectory.Replace("\\", "/");
                string guid = AssetDatabase.AssetPathToGUID(directory);
                AddressableAssetEntry assetEntry = settings.CreateOrMoveEntry(guid, group, false, false);
                assetEntry.address = directory;

                if (parentEntry != null)
                {
                    Debug.Log($"Processed Directory '{directory}' as child of Directory '{directoryPath}'");
                    assetEntry.IsSubAsset = true;
                    assetEntry.ParentEntry = parentEntry;
                    assetEntry.IsFolder = true;

                    if (parentEntry.SubAssets != null) parentEntry.SubAssets.Add(assetEntry);
                    else parentEntry.SubAssets = new List<AddressableAssetEntry>() { assetEntry };

                    assetEntry.labels.Clear();
                    assetEntry.SetLabel(directoryPath, true, true, true);
                    assetEntry.SetLabel("AudioDirectory", true, true, true);

                    ProcessDirectoryContent<AudioClip>(directory, manager.CLIP_LABEL, settings, group);
                    ProcessDirectoryContent<AudioOverride>(directory, manager.OVERRIDE_LABEL, settings, group);
                }

                ProcessAddressables(manager, directory, settings, group);
            }
        }

        private List<AddressableAssetEntry> ProcessDirectoryContent<T>(string directoryPath, string label, AddressableAssetSettings settings, AddressableAssetGroup group) where T : Object
        {
            T[] clips = AssetUtility.GetAssetsAtPath<T>(directoryPath);
            List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();
            foreach (T clip in clips)
            {
                string path = AssetDatabase.GetAssetPath(clip);
                string guid = AssetDatabase.AssetPathToGUID(path);
                AddressableAssetEntry clipEntry = settings.CreateOrMoveEntry(guid, group, false, false);
                entries.Add(clipEntry);
                clipEntry.labels.Clear();
                clipEntry.SetLabel(directoryPath, true, true);
                clipEntry.SetLabel(label, true, true);
            }

            return entries;
        }

        private void CreateOverrideCache(AudioManager manager)
        {
            Dictionary<string, AudioOverride> mapping = new Dictionary<string, AudioOverride>();

            // Set default
            string[] subdirectories = Directory.GetDirectories(manager.AudioDirectoryRoot, "*", SearchOption.AllDirectories);
            foreach (string subdirectoryUnfixed in subdirectories)
            {
                string subdirectory = subdirectoryUnfixed.Replace("\\", "/");
                mapping[subdirectory] = manager.DefaultGroupSettings;
            }

            Dictionary<AudioOverride, string> groupsToPath = AssetUtility.GetAssetsInDirectory<AudioOverride>(manager.AudioDirectoryRoot);

            foreach (var groupPath in groupsToPath)
            {
                string assetPath = groupPath.Value;
                // Load the AudioOverrideGroup
                AudioOverride group = groupPath.Key;

                if (group == null) continue;

                Debug.Log($"Propogating override for {assetPath}");

                string directory = Path.GetDirectoryName(assetPath);
                directory = directory.Replace("\\", "/");
                // This is an audio directory, add the mapping
                mapping[directory] = group;

                // Propagate this group mapping to all child directories unless they have their own mapping
                PropagateMappingToChildren(directory, group, mapping, manager.DefaultGroupSettings);
            }

            manager.SetOverrideCache(mapping);
        }

        private void PropagateMappingToChildren(string parentDirectory, AudioOverride parentGroup, Dictionary<string, AudioOverride> mapping, AudioOverride defaultGroup)
        {
            string[] subdirectories = Directory.GetDirectories(parentDirectory);
            foreach (string subdirectoryUnfixed in subdirectories)
            {
                string subdirectory = subdirectoryUnfixed.Replace("\\", "/");
                Debug.Log("Propogating");
                // If the child directory doesn't have its own mapping or its mapping is the default one, inherit from the parent
                if (!mapping.ContainsKey(subdirectory) || mapping[subdirectory] == defaultGroup)
                {
                    mapping[subdirectory] = parentGroup;
                    PropagateMappingToChildren(subdirectory, parentGroup, mapping, defaultGroup);
                }
            }
        }

        private void CreateGroupCache(AudioManager manager, AddressableAssetSettings settings)
        {
            Dictionary<string, AudioGroup> groupCache = new Dictionary<string, AudioGroup>();

            foreach (var overrideMapping in manager.OverrideCache)
            {
                string directoryPath = overrideMapping.Key;
                AudioOverride audioOverride = overrideMapping.Value;

                string[] clipPaths = AssetUtility.GetAssetPathsAtPath<AudioClip>(directoryPath);

                List<AssetReference> references = new List<AssetReference>();

                foreach (string path in clipPaths)
                {
                    // Convert the asset path to a GUID
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    // Construct an AssetReference using the GUID
                    AssetReference assetReference = new AssetReference(guid);
                    references.Add(assetReference);
                }

                AudioGroup group = new AudioGroup(audioOverride, references);
                groupCache[directoryPath] = group;
            }

            manager.SetGroupCache(groupCache);
        }
    }
}
