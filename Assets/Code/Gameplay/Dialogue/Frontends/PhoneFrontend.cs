using System.Collections;
using System.Collections.Generic;
using LostInLeaves.Dialogue;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.UI;
using DG.Tweening;
using CurlyCore.Input;
using CurlyCore;

namespace LostInLeaves
{
    [CreateAssetMenu(menuName = "Lost In Leaves/Dialogue/Phone Frontend", order = 0, fileName = "phone-frontend")]
    public class PhoneFrontend : DialogueFrontendObject
    {
        [SerializeField] private GameObject _textPrefab;
        [SerializeField] private float _backgroundColorAlpha = 0.5f;
        [SerializeField] private Color _backgroundColor = Color.white;
        [SerializeField] private float _fadeSpeed = 1f;
        [SerializeField] private float _textSpeed = 60f;
        [SerializeField, InputPath] private string _continuePrompt;

        private Canvas _rootCanvas;
        private GameObject _prefabInstance;
        private RectTransform _rectTransform;
        private TextMeshProUGUI _textInstance;
        private Image _background;
        [GlobalDefault] private InputManager _inputManager;

        public override async Task BeginDialogue()
        {
            if (_inputManager == null)
            {
                DependencyInjector.InjectDependencies(this);
            }

            if (_rootCanvas == null)
            {
                // Get gamehud via tag
                _rootCanvas = GameObject.FindGameObjectWithTag("GameHUD").GetComponent<Canvas>();
            }

            if (_prefabInstance == null)
            {
                Debug.Log("PhoneFrontend: Instantiating text prefab");
                _prefabInstance = Instantiate(_textPrefab);
                // find an image and text component somewhere in the prefab
                _textInstance = _prefabInstance.GetComponentInChildren<TextMeshProUGUI>();
                _rectTransform = _prefabInstance.GetComponent<RectTransform>();
                _background = _prefabInstance.GetComponentInChildren<Image>();
                _rectTransform.SetParent(_rootCanvas.transform, false);
            }

            if (_background) _background.color = _backgroundColor;

            SetBackgroundAlpha(0f);
            _textInstance.text = "";
            _prefabInstance.SetActive(true);

            await AnimateAlpha(_backgroundColorAlpha, _fadeSpeed);
            SetBackgroundAlpha(_backgroundColorAlpha);

        }

        public override async Task<int> DisplayNode(DialogueNode node)
        {
            _textInstance.text = "";
            await Typewriter.ApplyTo(_textInstance, node.Content, _textSpeed);

            // await for the user to press the continue button
            while (!_inputManager.GetInputDown(_continuePrompt))
            {
                await Task.Yield();
            }

            return 0;
        }

        public override async Task EndDialogue()
        {
            await AnimateAlpha(0f, _fadeSpeed);
            _textInstance.text = "";
            _prefabInstance.gameObject.SetActive(false);
        }

        private async Task AnimateAlpha(float targetAlpha, float duration)
        {
            if (_background == null) return;

            // skip if the alpha is already at the target
            if (Mathf.Approximately(_background.color.a, targetAlpha)) return;
            Debug.Log("Animating alpha to " + targetAlpha);
            // use dotween to animate the alpha
            await _background.DOFade(targetAlpha, duration).AsyncWaitForCompletion();
        }

        private void SetBackgroundAlpha(float alpha)
        {
            if (_background == null) return;

            Color color = _background.color;
            color.a = alpha;
            _background.color = color;
        }
    }
}
