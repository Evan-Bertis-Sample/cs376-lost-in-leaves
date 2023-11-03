using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using System.Linq;
using CurlyCore.Saving;
using CurlyEditor.Utility;

namespace CurlyEditor.Core
{
    [CustomPropertyDrawer(typeof(SerializerID))]
    public class SerializerIDDrawer : SearchBarDrawer
    {
        private class SerializerSearchWindowProvider : ScriptableObject, ISearchWindowProvider
        {
            public SerializerIDDrawer drawer;

            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                List<SearchTreeEntry> items = new List<SearchTreeEntry>();
                items.Add(new SearchTreeGroupEntry(new GUIContent("Select a Serializer ID"), 0));

                var ids = SerializerFactory.GetSerializerIDs();
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

        private SerializerSearchWindowProvider _searchProvider;

        public SerializerIDDrawer()
        {
            _searchProvider = ScriptableObject.CreateInstance<SerializerSearchWindowProvider>();
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

