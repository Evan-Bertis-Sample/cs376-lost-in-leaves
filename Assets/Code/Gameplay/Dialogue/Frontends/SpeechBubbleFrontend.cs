using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LostInLeaves.Dialogue
{
    [CreateAssetMenu(menuName ="Lost In Leaves/Dialogue/Speech Bubble Frontend")]
    public class SpeechBubbleFrontend : DialogueFrontendObject
    {
        public override Task BeginDialogue()
        {
            return Task.CompletedTask;
        }

        public override Task<int> DisplayNode(DialogueNode node)
        {
            Debug.Log("Displaying node");
            return Task.FromResult(0);
        }

        public override Task EndDialogue()
        {
            return Task.CompletedTask;   
        }
    }
}
