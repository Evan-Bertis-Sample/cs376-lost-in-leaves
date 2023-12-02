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
        private static HashSet<(DialogueEmitter, string)> _activeDialogues = new HashSet<(DialogueEmitter, string)>();

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

        public static async Task RunDialogue(DialogueEmitter emitter, string dialoguePath = "", bool failOnBusy = true)
        {
            GetEmitters();
            IDialogueFrontend frontend = emitter.DialogueFrontend;
            string path = string.IsNullOrEmpty(dialoguePath) ? emitter.DialoguePath : dialoguePath;

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("DialogueRunner: No dialogue path was provided.");
                return;
            }

            while (_activeDialogues.Count > 0 && failOnBusy)
            {
                Debug.LogWarning("DialogueRunner: Dialogue is already running. Adding to queue.");
                await Task.Delay(1000);
            }

            // add this conversation to the queue
            _activeDialogues.Add((emitter, path));

            DialogueTree tree = BuildDialogueTree(path, emitter.name);
            DialogueNode node = tree.Root;

            if (tree == null)
            {
                Debug.LogError("DialogueTree not set.");
                return;
            }

            if (frontend == null)
            {
                Debug.LogError("DialogueFrontend not set.");
                return;
            }


            frontend.CharacterName = emitter.CharacterName;
            frontend.AnchorTransform = emitter.gameObject.transform;

            Debug.Log("Beginning Dialogue");
            Task beginDialogueTask = frontend.BeginDialogue();
            await beginDialogueTask;

            Debug.Log("Running Dialogue");
            Task dialogueTask = TraverseDialogue(node, frontend);
            await dialogueTask;

            Debug.Log("Ending Dialogue");
            Task endDialogueTask = frontend.EndDialogue();
            await endDialogueTask;

            Debug.Log("Dialogue Complete");
            // wait for the next frame
            await Task.Yield();

            // dequeue this conversation
            _activeDialogues.Remove((emitter, path));
        }

        public static IEnumerator RunDialogueCoroutine(DialogueEmitter emitter, string dialoguePath = "", bool failOnBusy = true)
        {
            yield return new TaskUtility.WaitForTask(RunDialogue(emitter, dialoguePath, failOnBusy));
        }

        public static DialogueEmitter GetDialogueEmitter(string name)
        {
            GetEmitters();
            name = name.ToLower();
            if (_availableEmitters.ContainsKey(name))
            {
                return _availableEmitters[name];
            }
            else
            {
                Debug.LogError($"DialogueRunner: Could not find emitter for speaker {name}");
                return null;
            }
        }

        private static void GetEmitters()
        {
            // find all the dialogue emitters in the scene
            DialogueEmitter[] emitters = GameObject.FindObjectsOfType<DialogueEmitter>();

            // clear the dictionary -- get rid of any emitters that have been destroyed
            _availableEmitters.Clear();

            foreach (DialogueEmitter emitter in emitters)
            {
                if (!_availableEmitters.ContainsKey(emitter.CharacterName.ToLower()))
                {
                    Debug.Log($"DialogueRunner: Adding emitter {emitter.CharacterName.ToLower()}");
                    _availableEmitters.Add(emitter.CharacterName.ToLower(), emitter);
                }
            }
        }

        private static async Task TraverseDialogue(DialogueNode node, IDialogueFrontend frontend)
        {
            if (node == null) return;

            await Task.Yield(); // a frame between traversals 
            int choiceIndex = -1; // Default value when there are no choices
            Debug.Log($"DialogueRunner: Traversing node - {node.Speaker}: {node.Content}");

            foreach (DialogueNode child in node.Children)
                Debug.Log("Children: " + child.Content);

            // set the anchor position of the frontend based on the emitter
            GetEmitters();
            if (_availableEmitters.ContainsKey(node.Speaker))
            {
                if (_availableEmitters[node.Speaker].DialogueFrontend != null)
                {
                    IDialogueFrontend newFrontend = _availableEmitters[node.Speaker].DialogueFrontend;
                    if (frontend != newFrontend)
                    {
                        Debug.Log($"DialogueRunner: Setting frontend to {newFrontend}");
                        await frontend.EndDialogue(); // end the old frontend
                        await newFrontend.BeginDialogue(); // start the new one
                        frontend = newFrontend;
                    }
                }

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
                    if (node.Children.Count > 0)
                        await TraverseDialogue(node.Children[0], frontend); // don't display this node, just move on
                    else await frontend.EndDialogue(); // end the dialogue
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
                    if (node.Children.Count > 0)
                        await TraverseDialogue(node.Children[0], frontend); // continue on
                    else await frontend.EndDialogue(); // end the dialogue
                    // choiceIndex = await dialogueFrontend.DisplayNode(node, _characterName);
                    // await TraverseDialogue(node.Children[0]);
                    break;
                case DialogueNode.NodeType.Exit:
                    Debug.Log("Displaying dialogue node -- exit");
                    choiceIndex = await frontend.DisplayNode(node);
                    if (node.Children.Count > 0)
                        await TraverseDialogue(node.Children[0], frontend); // just go on to the next node
                    else await frontend.EndDialogue(); // end the dialogue
                    break;
                default:
                    Debug.Log("Displaying dialogue node -- standard text");
                    choiceIndex = await frontend.DisplayNode(node);
                    if (node.Children.Count > 0)
                        await TraverseDialogue(node.Children[0], frontend);
                    else await frontend.EndDialogue(); // end the dialogue
                    break;
            }
        }
    }
}
