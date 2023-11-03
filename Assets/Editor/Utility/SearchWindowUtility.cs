using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace CurlyEditor.Utility
{
    public static class SearchWindowUtility
    {
        public static List<SearchTreeEntry> ConvertLeafToSearchTree<T>(Leaf<T> root, GUIContent rootEntry = null)
        {
            List<SearchTreeEntry> searchTree = new List<SearchTreeEntry>();

            if (rootEntry != null) searchTree.Add(new SearchTreeGroupEntry(rootEntry, 0));

            foreach (Leaf<T> leaf in root.Children)
                AddLeafToSearchTree(leaf, searchTree, 1);

            Debug.Log(searchTree.Count);
            return searchTree;
        }

        private static void AddLeafToSearchTree<T>(Leaf<T> leaf, List<SearchTreeEntry> searchTree, int depth)
        {
            if (leaf == null)
            {
                return;
            }

            if (leaf.Children == null || leaf.Children.Count == 0)
            {
                // Adding the content as a SearchTreeEntry
                searchTree.Add(new SearchTreeEntry(new GUIContent(leaf.Content?.ToString() ?? "null"))
                {
                    level = depth,
                    userData = leaf.Content // You can associate the actual leaf with the entry if needed
                });
                return;
            }


            searchTree.Add(new SearchTreeGroupEntry(new GUIContent(leaf.Content?.ToString() ?? "null"), depth));

            // Recursively adding children
            foreach (var child in leaf.Children)
            {
                if (child == null) continue;
                AddLeafToSearchTree(child, searchTree, depth + 1);
            }
        }
    }
}
