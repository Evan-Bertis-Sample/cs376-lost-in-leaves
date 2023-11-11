using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LostInLeaves.Dialogue
{
    public class DialogueTree
    {
        public DialogueNode Root { get; private set; }

        public DialogueTree(string textFileName)
        {
            BuildDialogueTree(textFileName);
        }

        private void BuildDialogueTree(string textFileName)
        {
            Debug.Log("Attempting to load dialogue tree from " + textFileName + ".");
            TextAsset textAsset = Resources.Load<TextAsset>(textFileName);
            if (textAsset == null)
            {
                Debug.LogError("Dialogue tree file not found at " + textFileName + ".");
                return;
            }

            string instructions = textAsset.text;

            string[] lines = instructions.Split('\n');

            Dictionary<string, DialogueNode> namedBranches = new Dictionary<string, DialogueNode>();
            Stack<DialogueNode> nodeStack = new Stack<DialogueNode>();
            Stack<DialogueNode> branchStack = new Stack<DialogueNode>();
            List<DialogueNode> exitsToResolve = new List<DialogueNode>();
            DialogueNode currentOptionBuilding = null;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed[0] == '#') continue;

                // Debug.Log("Parsing line: " + trimmed);

                DialogueNode node;
                if (IsMeta(trimmed))
                {
                    // handle meta commands
                    node = BuildMetaNode(trimmed);
                }
                else
                {
                    // this is just a dialogue node
                    node = new DialogueNode(trimmed, new List<DialogueNode>(), null);
                }

                // check if this is the first node
                if (Root == null && node.Type != DialogueNode.NodeType.Content)
                {
                    Debug.LogError("Dialogue tree has a meta command before the first content node, quitting.");
                    return;
                }
                if (Root == null && node.Type == DialogueNode.NodeType.Content)
                {
                    // this is the first node, so we should set it as the root
                    Root = node;
                    nodeStack.Push(node);
                    continue;
                }

                // Now we have a node, that isn't the first, we need to add it to the tree
                // But first, we should do some checks on the token type

                switch (node.Type)
                {
                    case DialogueNode.NodeType.Content:
                        // this is a content node, so we should add it to the current node
                        if (exitsToResolve.Count > 0 && currentOptionBuilding == null)
                        {
                            Debug.Log($"Resolving {exitsToResolve.Count} Exits!");
                            foreach (DialogueNode exit in exitsToResolve)
                                exit.Children.Add(node);
                            
                            exitsToResolve.Clear();
                        }
                        else
                            nodeStack.Peek().Children.Add(node);
                        nodeStack.Push(node);
                        break;
                    case DialogueNode.NodeType.Branch:
                        // we are creating a branch, so we should do nothing, and wait for the next node
                        // except we should add this node to the named branches
                        // check if the branch has a name
                        nodeStack.Peek().Children.Add(node);
                        branchStack.Push(node);
                        if (node.Parameters == null || node.Parameters.Count == 0)
                        {
                            break;
                        }
                        string branchName = (string)node.Parameters[0];
                        namedBranches.Add(branchName, node);
                        break;
                    case DialogueNode.NodeType.Option:
                        // we are adding options to a branch, so we should add this node to the branch
                        // now we should just add the option to whatever the last node in the stack was
                        branchStack.TryPeek(out DialogueNode branch);
                        if (branch == null)
                        {
                            Debug.LogError("Dialogue tree has an option node without a branch, quitting.");
                            return;
                        }
                        branch.Children.Add(node);
                        nodeStack.Push(node); // we should push this node onto the stack, so that the next node will be added to this one
                        currentOptionBuilding = node;
                        break;
                    case DialogueNode.NodeType.Event:
                        // This is just a content node
                        nodeStack.Peek().Children.Add(node);
                        nodeStack.Push(node);
                        break;
                    case DialogueNode.NodeType.Exit:
                        // we should go back a branch
                        if (node.Parameters != null && node.Parameters.Count > 0)
                        {
                            // then we should go back to a named branch
                            string branchToGoBackTo = (string)node.Parameters[0];
                            // pop the branch stack until we find the branch
                            while (branchStack.TryPop(out DialogueNode poppedBranch))
                            {
                                if (poppedBranch == null)
                                {
                                    Debug.LogError("Dialogue tree has an exit node without a branch!.");
                                    return;
                                }
                                if (poppedBranch == namedBranches[branchToGoBackTo])
                                {
                                    // we found the branch, so we should stop popping
                                    // set this branch as the child of this node
                                    node.Children.Add(poppedBranch);
                                    branchStack.Push(poppedBranch); // we actually didn't want to remove this one
                                    // also set this to be the current node
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // we want to connect this to the next content node in the line
                            exitsToResolve.Add(node);
                        }

                        // now we should add this node to the previous node
                        nodeStack.Peek().Children.Add(node);
                        currentOptionBuilding = null;
                        break;
                }
            }
            // Debug.Log(PrintTree());
        }

        private string PrintTree(DialogueNode node = null, int indentLevel = 0)
        {
            // print the tree
            if (node == null && indentLevel == 0) node = Root;
            string message = "";

            if (node == null) return message;

            // if (node.Type == DialogueNode.NodeType.Exit) return message;

            string typename = node.Type.ToString();
            message += new string(' ', indentLevel * 4);
            message += typename + ": ";
            message += node.Content;
            message += "\n";

            if (node.Type == DialogueNode.NodeType.Option || node.Type == DialogueNode.NodeType.Branch) indentLevel++;
            foreach (var child in node.Children)
                message += PrintTree(child, indentLevel);

            return message;
        }

        private bool IsMeta(string line)
        {
            return line[0] == '[' && line[line.Length - 1] == ']';
        }

        private DialogueNode BuildMetaNode(string token)
        {
            // tokens should look like this [command(parameters)] or [command]
            // remove this
            // extract the command
            string command = token.Substring(1, token.IndexOf(']') - 1);

            // extract the parameters

            if (command.Contains("(") && !command.Contains(")"))
            {
                Debug.LogError("Dialogue tree has a meta command with mismatched parentheses, quitting.");
                return null;
            }

            if (!command.Contains("(") && command.Contains(")"))
            {
                Debug.LogError("Dialogue tree has a meta command with mismatched parentheses, quitting.");
                return null;
            }

            if (!command.Contains("(") && !command.Contains(")"))
            {
                // this is a command with no parameters
                return new DialogueNode(DialogueNode.GetNodeType(command), new List<DialogueNode>(), null);
            }

            string paramsAll = token.Substring(token.IndexOf('(') + 1, token.IndexOf(')') - token.IndexOf('(') - 1);
            List<string> paremeters = new List<string>(paramsAll.Split(','));

            // trim the parameters
            for (int i = 0; i < paremeters.Count; i++)
            {
                paremeters[i] = paremeters[i].Trim();
            }
            string commandName = command.Substring(0, command.IndexOf('('));
            DialogueNode.NodeType nodeType = DialogueNode.GetNodeType(commandName);
            List<object> parameterObject = new List<object>();
            foreach (string parameter in paremeters)
            {
                if (parameter == "") continue;
                parameterObject.Add(ParseParameter(parameter));
            }

            // now build the node
            DialogueNode node = new DialogueNode(nodeType, new List<DialogueNode>(), null, parameterObject);
            return node;
        }

        private object ParseParameter(string parameter)
        {
            Debug.Log("Dialogue Tree : Parsing parameter - " + parameter + ".");
            // if it's a number
            if (parameter[0] >= '0' && parameter[0] <= '9')
            {
                // if it's an int
                if (parameter.Contains('.'))
                {
                    return float.Parse(parameter);
                }
                else
                {
                    return int.Parse(parameter);
                }
            }
            else if (parameter[0] == '"') // if it's a string
            {
                return parameter.Substring(1, parameter.Length - 2);
            }
            else if (parameter[0] == '[') // if it's a list
            {
                // remove the brackets
                parameter = parameter.Substring(1, parameter.Length - 2);

                // split the list
                string[] list = parameter.Split(',');

                // trim the list
                for (int i = 0; i < list.Length; i++)
                {
                    list[i] = list[i].Trim();
                }

                return list;
            }
            else // if it's a bool
            {
                return bool.Parse(parameter);
            }
        }
    }
}
