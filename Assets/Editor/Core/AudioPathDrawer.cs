using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

using CurlyCore.CurlyApp;
using CurlyCore.Audio;
using CurlyUtility;
using CurlyEditor.Utility;
using System.Reflection;
using CurlyCore;

namespace CurlyEditor.Core
{
    [CustomPropertyDrawer(typeof(AudioPath))]
    public class AudioPathDrawer : SearchBarDrawer
    {
        public class AudioSearchProvider : ScriptableObject, ISearchWindowProvider
        {
            public Leaf<AudioLeafContent> Root;
            private Action<string> _onSelect;

            public AudioSearchProvider(Leaf<AudioLeafContent> root, Action<string> onSelect = null)
            {
                Root = root;
                _onSelect = onSelect;
            }

            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                return SearchWindowUtility.ConvertLeafToSearchTree<AudioLeafContent>(Root, new GUIContent("Search"));
            }

            public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
            {
                AudioLeafContent content = SearchTreeEntry.userData as AudioLeafContent;
                _onSelect?.Invoke(content.Path);
                return true;
            }
        }

        public class AudioLeafContent
        {
            public string DisplayName;
            public string Path;

            public override string ToString()
            {
                return DisplayName;
            }
        }

        private AudioManager _manager;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            object targetObject = property.serializedObject.targetObject;
            _manager = ReflectionUtility.GetFieldByType<AudioManager>(targetObject);
            if (_manager == null) _manager = GlobalDefaultStorage.GetDefault(typeof(AudioManager)) as AudioManager;
        }

        protected override void ButtonClicked(Rect buttonPosition)
        {
            Leaf<AudioLeafContent> content = GenerateContent();
            AudioSearchProvider provider = new AudioSearchProvider(content, UpdateProperty);
            SearchWindowContext searchContext = new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
            SearchWindow.Open(searchContext, provider);
        }

        // Need to fix the bug where audioclips are displayed alongside of the folder they are located in
        private Leaf<AudioLeafContent> GenerateContent()
        {
            List<Leaf<AudioLeafContent>> children = GetChildren(_manager.AudioDirectoryRoot);

            return new Leaf<AudioLeafContent>(null, children);
        }

        private List<Leaf<AudioLeafContent>> GetChildren(string path)
        {
            List<Leaf<AudioLeafContent>> children = new List<Leaf<AudioLeafContent>>();

            string[] subdirectories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < subdirectories.Length; i++)
            {
                string subdirectory = subdirectories[i].Replace("\\", "/");
                List<Leaf<AudioLeafContent>> leafChildren = GetChildren(subdirectory);

                AudioOverride over = _manager.GetOverride(subdirectory);

                if (over != null && over.IdentifyByFileName)
                {
                    // Add all of the files
                    Dictionary<AudioClip, string> pathsByAsset = AssetUtility.GetAssetsInDirectory<AudioClip>(subdirectory);
                    if (leafChildren == null && pathsByAsset.Count > 0) leafChildren = new List<Leaf<AudioLeafContent>>();
                    foreach (var pair in pathsByAsset)
                    {
                        string filename = GetLast(pair.Value);
                        AudioLeafContent leafContent = new AudioLeafContent { DisplayName = filename, Path = pair.Value };
                        Leaf<AudioLeafContent> assetLeaf = new Leaf<AudioLeafContent>(leafContent, null);
                        leafChildren.Add(assetLeaf);
                    }
                }

                string dir = GetLast(subdirectory);
                AudioLeafContent content = new AudioLeafContent() { DisplayName = dir, Path = subdirectory };
                Leaf<AudioLeafContent> child = new Leaf<AudioLeafContent>(content, leafChildren);

                children.Add(child);
            }

            return children;
        }

        private void UpdateProperty(string value)
        {
            _propertyValue = value;
        }

        private string GetLast(string path)
        {
            string last = path.Split('/').Last();
            Debug.Log($"Last of {path} is {last}");
            return last;
        }
    }
}
