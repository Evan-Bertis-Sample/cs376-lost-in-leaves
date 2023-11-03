using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using System.Linq;
using CurlyCore.Saving;
using CurlyEditor.Utility;

namespace CurlyEditor.Core
{
    [CustomPropertyDrawer(typeof(EncryptorID))]
    public class EncryptorIDDrawer : SearchBarDrawer
    {
        private class EncryptorSearchWindowProvider : ScriptableObject, ISearchWindowProvider
        {
            public EncryptorIDDrawer drawer;

            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                List<SearchTreeEntry> items = new List<SearchTreeEntry>();
                items.Add(new SearchTreeGroupEntry(new GUIContent("Select an Encryptor ID"), 0));

                var ids = EncryptorFactory.GetEncryptorIDs();
                foreach (var id in ids)
                {
                    items.Add(new SearchTreeEntry(new GUIContent(id))
                    {
                        level = 1,
                        userData = id
                    });
                }

                return items;
            }

            public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
            {
                if (entry.userData is string selectedId)
                {
                    drawer.UpdateProperty(selectedId);
                    return true;
                }
                return false;
            }
        }

        private EncryptorSearchWindowProvider _searchProvider;

        public EncryptorIDDrawer()
        {
            _searchProvider = ScriptableObject.CreateInstance<EncryptorSearchWindowProvider>();
            _searchProvider.drawer = this;
        }

        protected override void ButtonClicked(Rect buttonPosition)
        {
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), _searchProvider);
        }

        public void UpdateProperty(string selectedId)
        {
            _propertyValue = selectedId;
        }
    }
}
