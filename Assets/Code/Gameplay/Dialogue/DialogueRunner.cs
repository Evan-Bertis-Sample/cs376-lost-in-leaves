using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurlyUtility;
using UnityEngine;

namespace LostInLeaves.Dialogue
{
    public class DialogueRunner
    {
        public delegate void DialogueEventHandler(string eventName, object[] parameters);
        public static event DialogueEventHandler OnDialogueEvent;

        private static Dictionary<string, DialogueEmitter> _availableEmitters = new Dictionary<string, DialogueEmitter>();

        public static DialogueTree BuildDialogueTree(string path, string defaultSpeaker)
        {
            Debug.Log($"DialogueRunner : Building dialogue tree from '{path}'"); // Log the path of the dialogue tree
            string actualPath = path.Replace("Assets/Resources/", "");
            actualPath = actualPath.Replace(".txt", ""); // Remove the .txt extension
            // Initialize and load dialogue tree
            if (!string.IsNullOrEmpty(actualPath))
            {
                DialogueTree dialogueTree = new DialogueTree(actualPath, defaultSpeaker);
                return dialogueTree;
            }

            return null;
        }

        public static IEnumerator RunDialogueCoroutine(DialogueEmitter emitter)
        {
            GetEmitters();
            IDialogueFrontend frontend = emitter.DialogueFrontend;
            DialogueTree tree = BuildDialogueTree(emitter.DialoguePath, emitter.name);
            DialogueNode node = tree.Root;

            if (tree == null || frontend == null)
            {
                Debug.LogError("DialogueTree or DialogueFrontend is not set.");
                yield break;
            }

            if (tree == null || frontend == null)
            {
                Debug.LogError("DialogueTree or DialogueFrontend is not set.");
                yield break;
            }

            frontend.CharacterName = emitter.CharacterName;
            frontend.AnchorTransform = emitter.gameObject.transform;

            Debug.Log("Beginning Dialogue");
            Task beginDialogueTask = frontend.BeginDialogue();
            yield return new TaskUtility.WaitForTask(beginDialogueTask);

            Debug.Log("Running Dialogue");
            Task dialogueTask = TraverseDialogue(node, frontend);
            yield return new TaskUtility.WaitForTask(dialogueTask);

            Debug.Log("Ending Dialogue");
            Task endDialogueTask = frontend.EndDialogue();
            yield return new TaskUtility.WaitForTask(endDialogueTask);

            yield return null; // wait for the next frame

        }

        private static void GetEmitters()
        {
            // find all the dialogue emitters in the scene
            DialogueEmitter[] emitters = GameObject.FindObjectsOfType<DialogueEmitter>();

            // clear the dictionary -- get rid of any emitters that have been destroyed
            _availableEmitters.Clear();

            foreach (DialogueEmitter emitter in emitters)
            {
                if (!_availableEmitters.ContainsKey(emitter.name))
                {
                    _availableEmitters.Add(emitter.name, emitter);
                }
            }
        }

        private static async Task TraverseDialogue(DialogueNode node, IDialogueFrontend frontend)
        {
            if (node == null) return;

            await Task.Yield(); // a frame between traversals 
            int choiceIndex = -1; // Default value when there are no choices
            Debug.Log($"Traversing dialogue node -- {node.Content}");

            foreach (DialogueNode child in node.Children)
                Debug.Log("Children: " + child.Content);

            // set the anchor position of the frontend based on the emitter
            if (_availableEmitters.ContainsKey(node.Speaker))
            {
                frontend.AnchorTransform = _availableEmitters[node.Speaker].AnchorTransform;
                frontend.CharacterName = node.Speaker; // set the character name of the frontend
            }
            else
            {
                Debug.LogError($"DialogueRunner: Could not find emitter for speaker {node.Speaker}");
            }

            // If there are choices, follow the choice path, otherwise, iterate through all children
            switch (node.Type)
            {
                case DialogueNode.NodeType.Branch:
                    Debug.Log("Displaying dialogue node -- branch");
                    choiceIndex = await frontend.DisplayNode(node);
                    if (choiceIndex == -1)
                    {
                        Debug.LogError("No valid option was chosen!");
                    }
                    await TraverseDialogue(node.Children[choiceIndex], frontend);
                    break;
                case DialogueNode.NodeType.Option:
                    Debug.Log("Displaying dialogue node -- option");
                    await TraverseDialogue(node.Children[0], frontend); // don't display this node, just move on
                    break;
                case DialogueNode.NodeType.Event:
                    Debug.Log("Displaying dialogue node -- event");
                    // we should fire off an event here
                    // event name
                    string eventName = node.Parameters[0] as string;
                    // event parameters
                    object[] parameters = new object[node.Parameters.Count - 1];
                    if (parameters.Length > 0)
                    {
                        for (int i = 1; i < node.Parameters.Count; i++)
                        {
                            parameters[i - 1] = node.Parameters[i];
                        }
                    }
                    Debug.Log("Firing DialogueEvent: " + eventName + " with parameters: " + parameters.Length + " parameters.");
                    OnDialogueEvent?.Invoke(eventName, parameters);
                    await TraverseDialogue(node.Children[0], frontend); // continue on
                    // choiceIndex = await dialogueFrontend.DisplayNode(node, _characterName);
                    // await TraverseDialogue(node.Children[0]);
                    break;
                case DialogueNode.NodeType.Exit:
                    Debug.Log("Displaying dialogue node -- exit");
                    choiceIndex = await frontend.DisplayNode(node);
                    await TraverseDialogue(node.Children[0], frontend); // just go on to the next node
                    break;
                default:
                    Debug.Log("Displaying dialogue node -- standard text");
                    choiceIndex = await frontend.DisplayNode(node);
                    await TraverseDialogue(node.Children[0], frontend);
                    break;
            }
        }
    }
}
