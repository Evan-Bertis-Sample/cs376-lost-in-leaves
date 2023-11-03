using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEditor.Experimental.GraphView;

using CurlyCore.CurlyApp;
using CurlyCore.Input;
using CurlyEditor.Utility;
using System.Reflection;
using CurlyCore;

namespace CurlyEditor.Core
{
    [CustomPropertyDrawer(typeof(InputPathAttribute))]
    public class InputPathDrawer : SearchBarDrawer
    {
        public class InputSearchProvider : ScriptableObject, ISearchWindowProvider
        {
            public Leaf<InputLeafContent> Root;
            private Action<string> _onSelect;

            public InputSearchProvider(Leaf<InputLeafContent> root, Action<string> onSelect = null)
            {
                Root = root;
                _onSelect = onSelect;
            }

            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                return SearchWindowUtility.ConvertLeafToSearchTree<InputLeafContent>(Root, new GUIContent("Search"));
            }

            public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
            {
                InputLeafContent content = SearchTreeEntry.userData as InputLeafContent;
                _onSelect?.Invoke(content.Path);
                return true;
            }
        }

        public class InputLeafContent
        {
            public string DisplayName;
            public string Path;

            public override string ToString()
            {
                return DisplayName;
            }
        }

        private InputManager _manager;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            // Fetch manager from the object
            object targetObject = property.serializedObject.targetObject;
            _manager = ReflectionUtility.GetFieldByType<InputManager>(targetObject);
            if (_manager == null) _manager = GlobalDefaultStorage.GetDefault(typeof(InputManager)) as InputManager;

            if (!_manager.IsInputAssigned(property.stringValue))
            {
                Rect warningPosition = new Rect(position.x, position.y + _standardPropertyHeight * 1.25f, position.width, _standardPropertyHeight * 1.75f);
                EditorGUI.HelpBox(warningPosition, $"Input: '{property.stringValue}' is not assigned in Input Manager!", MessageType.Warning);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_manager == null) return _standardPropertyHeight;
            if (_manager.IsInputAssigned(_propertyValue)) return _standardPropertyHeight;
            return _standardPropertyHeight * 3f;
        }

        protected override void ButtonClicked(Rect buttonPosition)
        {
            Leaf<InputLeafContent> content = GenerateBrowserContent();
            InputSearchProvider provider = new InputSearchProvider(content, UpdateProperty);
            SearchWindowContext searchContext = new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
            SearchWindow.Open(searchContext, provider);
        }

        private Leaf<InputLeafContent> GenerateBrowserContent()
        {
            if (_manager.MasterActionAsset == null) throw new System.Exception("Action map is not defined.");
            IEnumerable<InputActionMap> actionMaps = _manager.MasterActionAsset.actionMaps;

            List<Leaf<InputLeafContent>> children = new List<Leaf<InputLeafContent>>();

            foreach (InputActionMap map in actionMaps)
            {
                List<InputLeafContent> mapChildren = new List<InputLeafContent>();
                foreach (InputAction action in map.actions)
                {
                    InputLeafContent content = new InputLeafContent() { DisplayName = action.name, Path = $"{map.name}/{action.name}" };
                    mapChildren.Add(content);
                }

                InputLeafContent mapContent = new InputLeafContent() { DisplayName = map.name, Path = map.name };
                children.Add(new Leaf<InputLeafContent>(mapContent, mapChildren, null));
            }

            return new Leaf<InputLeafContent>(null, children);
        }

        private void UpdateProperty(string value)
        {
            _propertyValue = value;
            _manager = null;
        }
    }
}