using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurlyCore;
using CurlyCore.Audio;
using CurlyCore.Input;
using CurlyUtility;
using LostInLeaves.Components;
using UnityEngine;

namespace LostInLeaves.Dialogue
{
    [CreateAssetMenu(menuName = "Lost In Leaves/Dialogue/Speech Bubble Frontend")]
    public class SpeechBubbleFrontend : DialogueFrontendObject
    {
        [SerializeField] private SpeechBubble _speechBubblePrefab;
        [SerializeField] private float _charactersPerSecond = 60;
        [SerializeField, InputPath] private string _continuePrompt;
        [SerializeField] private Vector3 _anchorOffset = new Vector3(0, 1.5f, 0);

        public Vector3 SpeechPosition => AnchorTransform.position + _anchorOffset;

        [GlobalDefault] private InputManager _inputManager;
        [GlobalDefault] private VoiceBank _voiceBank;

        private SpeechBubble _speechBubbleInstance;
        private string _characterName;

        public override Task BeginDialogue()
        {
            if (_inputManager == null || _voiceBank == null)
            {
                DependencyInjector.InjectDependencies(this);
            }

            if (_speechBubbleInstance == null)
            {
                _speechBubbleInstance = Instantiate(_speechBubblePrefab);
            }

            _speechBubbleInstance.SetText("");
            _speechBubbleInstance.gameObject.SetActive(true);
            _speechBubbleInstance.transform.position = SpeechPosition;
            _speechBubbleInstance.SetBubblePosition(AnchorTransform, _anchorOffset);
            _speechBubbleInstance.SetTailTarget(AnchorTransform.position);

            return Task.CompletedTask;
        }

        public override async Task<int> DisplayNode(DialogueNode node)
        {
            _characterName = node.Speaker;
            switch (node.Type)
            {
                case DialogueNode.NodeType.Content:
                    await Typewriter.ApplyTo(_speechBubbleInstance.TextMesh, node.Content, _charactersPerSecond,
                                            inputManager: _inputManager, inputPrompt: _continuePrompt, onReveal: OnReveal);

                    if ((node.Children.Count > 0 && node.Children[0].Type != DialogueNode.NodeType.Branch) || node.Children.Count == 0)
                    {
                        await TaskUtility.WaitUntil(() =>
                        {
                            // Debug.Log("Waiting for continue prompt");
                            _speechBubbleInstance.SetBubblePosition(AnchorTransform, _anchorOffset);
                            return _inputManager.GetInputDown(_continuePrompt);
                        });
                    }
                    break;
                default:
                    Debug.LogError($"Node type {node.Type} is not supported by SpeechBubbleFrontend");
                    break;
            }

            return await Task.FromResult(0);
        }

        private void OnReveal(char c)
        {
            _speechBubbleInstance.SetBubblePosition(AnchorTransform, _anchorOffset);
            _speechBubbleInstance.Render(AnchorTransform.position);
            _voiceBank.PlayVoice(_characterName, c, SpeechPosition);
        }

        public override Task EndDialogue()
        {
            _speechBubbleInstance.SetText("");
            _speechBubbleInstance.gameObject.SetActive(false);
            return Task.CompletedTask;
        }
    }
}
