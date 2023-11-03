using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CurlyCore.SceneManagement.Transitions
{
    [CreateAssetMenu(fileName = "FadeTransition", menuName = "Curly/Scene Transitions/Fade Transition")]
    public class FadeTransition : ScreenTransitionObject
    {
        public Color color = Color.black;
        [Range(0, 1)] public float StartingOpacity = 0;
        [Range(0, 1)] public float EndingOpacity = 1;
        public float fadeDuration = 1f;

        private Image _fadeImage;
        private Tweener _fadeTween;

        public override void PrepareTransition(Canvas screenCanvas)
        {
            GameObject fadeObject = new GameObject("FadeObject");
            fadeObject.transform.SetParent(screenCanvas.transform, false);

            _fadeImage = fadeObject.AddComponent<Image>();
            _fadeImage.color = new Color(color.r, color.g, color.b, StartingOpacity);
            _fadeImage.rectTransform.anchoredPosition = Vector2.zero;
            _fadeImage.rectTransform.sizeDelta = new Vector2(screenCanvas.pixelRect.width, screenCanvas.pixelRect.height);
            _fadeImage.rectTransform.anchorMin = Vector2.zero;
            _fadeImage.rectTransform.anchorMax = Vector2.one;
        }

        public override async Task Transition(Canvas screenCanvas)
        {
            _fadeTween = _fadeImage.DOFade(EndingOpacity, fadeDuration);
            await _fadeTween.AsyncWaitForCompletion();
        }

        public override void EndTransition(Canvas screenCanvas)
        {
            _fadeTween?.Kill();
            if (_fadeImage != null)
            {
                Destroy(_fadeImage.gameObject);
                _fadeImage = null;
            }
        }
    }
}
