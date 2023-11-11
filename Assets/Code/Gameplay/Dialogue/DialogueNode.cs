using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInLeaves.Dialogue
{
    public class DialogueNode
    {
        public enum NodeType { Content, Branch, Option, Event, Exit }
        public NodeType Type { get; private set; }
        public string Content { get; private set; }
        public List<object> Parameters { get; private set; }
        public List<DialogueNode> Children { get; private set; }
        public DialogueNode Parent { get; private set; }
        public bool IsMeta { get; private set; }

        public DialogueNode(string content, List<DialogueNode> children, DialogueNode parent = null)
        {
            Type = NodeType.Content;
            Content = content; // if this is a meta node, this is the command
            Children = children;
            Parent = parent;
            IsMeta = false;
        }

        public DialogueNode(NodeType nodeType, List<DialogueNode> children, DialogueNode parent = null, List<object> parameters = null)
        {
            Type = nodeType;
            Content = nodeType.ToString();
            Children = children;
            Parent = parent;
            Parameters = parameters;
            IsMeta = true;
        }

        public static NodeType GetNodeType(string name)
        {
            Dictionary<string, NodeType> mappings = new Dictionary<string, NodeType>()
            {
                {"content", NodeType.Content},
                {"branch", NodeType.Branch},
                {"option", NodeType.Option},
                {"event", NodeType.Event},
                {"exit", NodeType.Exit}
            };

            if (mappings.ContainsKey(name.ToLower()))
            {
                return mappings[name.ToLower()];
            }
            else
            {
                Debug.Log("couldn't find node type for " + name + ", defaulting to content");
                return NodeType.Content;
            }

        }
    }
}

