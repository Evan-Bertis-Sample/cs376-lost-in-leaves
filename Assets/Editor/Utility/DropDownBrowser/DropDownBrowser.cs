using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace CurlyEditor.Utility
{
    public class DropDownBrowser : PopupWindowContent
    {
        private Leaf<string> _content = null;
        private Leaf<string> _currentContent = null;
        public string CurrentPath { get; private set; }

        public delegate void BrowserUpdate(string path);
        public event BrowserUpdate PathUpdate;
        private Vector2 scrollPos = Vector2.zero;
        public DropDownBrowser(Leaf<string> content)
        {
            _content = content;
            _currentContent = content;
            CurrentPath = "";
        }

        public override void OnGUI(Rect rect)
        {
            if (_currentContent == null || _currentContent.Children == null || !_currentContent.Children.Any(x => x != null))
            {
                editorWindow.Close();
                return;
            }
            DrawChildSelect(_currentContent);
        }

        public void DrawChildSelect(Leaf<string> root)
        {
            if (!root.Children.Any(x => x != null)) return;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach (Leaf<string> child in root.Children)
            {
                if (child == null) continue;
                if (GUILayout.Button(child.Content))
                {
                    UpdatePath(child.Content);
                    _currentContent = child;
                }
            }
            EditorGUILayout.EndScrollView();
        }

        public void UpdatePath(string leafName)
        {
            if (CurrentPath == "")
            {
                CurrentPath = leafName;
                PathUpdate?.Invoke(CurrentPath);
                return;
            }

            CurrentPath += $"/{leafName}";
            PathUpdate?.Invoke(CurrentPath);
        }

        public override void OnClose()
        {
            Delegate[] clients = PathUpdate.GetInvocationList();
            if (clients != null)
            {
                foreach (Delegate client in clients)
                {
                    PathUpdate -= client as BrowserUpdate;
                }
            }
        }
    }
}