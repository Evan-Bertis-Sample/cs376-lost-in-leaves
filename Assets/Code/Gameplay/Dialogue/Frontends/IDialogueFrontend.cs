using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LostInLeaves.Dialogue
{
    public interface IDialogueFrontend
    {
        string CharacterName { get; set; }
        Transform AnchorTransform { get; set; }

        Task BeginDialogue();
        Task<int> DisplayNode(DialogueNode node); // Updated to return an int
        Task EndDialogue();
    }

    public abstract class DialogueFrontendObject : ScriptableObject, IDialogueFrontend
    {
        public string CharacterName { get; set; }
        public Transform AnchorTransform { get; set; }

        public virtual async Task BeginDialogue() {}
        public virtual async Task<int> DisplayNode(DialogueNode node) {return -1; } // Must return an int
        public virtual async Task EndDialogue() {}
    }
}
