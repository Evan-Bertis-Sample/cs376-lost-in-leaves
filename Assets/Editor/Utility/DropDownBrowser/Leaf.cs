using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurlyEditor.Utility
{
    [System.Serializable]
    public class Leaf<T>
    {
        [field: SerializeField] public List<Leaf<T>> Children { get; private set; } = new List<Leaf<T>>();
        [field: SerializeField] public T Content { get; private set; }
        public Leaf(T content, List<Leaf<T>> children)
        {
            Content = content;
            Children = children;
        }

        public Leaf(T content, List<T> childContents, T defaultGrandchildren)
        {
            List<Leaf<T>> children = new List<Leaf<T>>(childContents.Count);
            foreach (T currentContent in childContents)
            {
                children.Add(new Leaf<T>(currentContent, null));
            }

            Children = children;
            Content = content;
        }
    }
}