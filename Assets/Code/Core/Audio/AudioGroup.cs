using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CurlyCore.Audio
{
    [System.Serializable]
    public struct AudioGroup
    {
        [System.Serializable]
        public class StringToReferenceMapping : SerializableDictionary<string, AssetReference> { }

        public AudioOverride Override;
        [field: SerializeField] public List<AssetReference> AudioReferences { get; private set; }
        [field: SerializeField] public StringToReferenceMapping PathToReference;

        public AudioGroup(AudioOverride over, List<AssetReference> references)
        {
            Override = over;
            AudioReferences = references;

            PathToReference = new StringToReferenceMapping();

#if UNITY_EDITOR
            PathToReference = GeneratePathDictionary(references);
#endif
        }

        public AssetReference GetReference()
        {
            return AudioReferences[0];
        }

        public AssetReference ChooseRandom()
        {
            if (AudioReferences.Count == 0) return null;
            return AudioReferences[Random.Range(0, AudioReferences.Count - 1)];
        }

        public AssetReference ChooseClip(string clipPath)
        {
            PathToReference.TryGetValue(clipPath, out AssetReference value);
            return value;
        }

#if UNITY_EDITOR
        private StringToReferenceMapping GeneratePathDictionary(List<AssetReference> references)
        {
            StringToReferenceMapping mapping = new StringToReferenceMapping();

            foreach (AssetReference reference in references)
            {
                string path = AssetDatabase.GUIDToAssetPath(reference.AssetGUID);
                Debug.Log($"Adding {path}, {reference}");
                mapping.Add(path, reference);
            }
            Debug.Log(mapping.Count);
            return mapping;
        }
#endif
    }
}
