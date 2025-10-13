using System.Collections;
using UnityEngine;
using TMPro;

namespace RabbitDog.UIExtensions
{
    public class AlertMessage : CachedMonoBehaviour
    {
        [SerializeField] TextMeshProUGUI alertText;
        [SerializeField] RectTransform alertRectTr;
        [SerializeField] CanvasGroup alertGroup;

        public string Message { get; private set; }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAllCoroutines();
        }

        public void Show(string message, Color textColor)
        {
            StopAllCoroutines();
            Message = message;
            alertText.text = message;
            alertText.color = textColor;
            alertGroup.alpha = 0f;
            alertRectTr.anchoredPosition = new Vector2(0, -50f);
            StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            // Fade in and move up
            yield return Animate(0f, 1f, -50f, 0f, 0.5f);

            // Wait
            yield return new WaitForSeconds(1f);

            // Fade out and move down
            yield return Animate(1f, 0f, 0f, 50f, 0.5f);

            AnimationEndCallback();
        }

        private IEnumerator Animate(float startAlpha, float endAlpha, float startY, float endY, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                // Ease OutSine
                t = Mathf.Sin(t * Mathf.PI * 0.5f);
                alertGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                float y = Mathf.Lerp(startY, endY, t);
                alertRectTr.anchoredPosition = new Vector2(0, y);
                elapsed += Time.deltaTime;
                yield return null;
            }
            alertGroup.alpha = endAlpha;
            alertRectTr.anchoredPosition = new Vector2(0, endY);
        }

        void AnimationEndCallback()
        {
            Message = null;
            AlertMessageManager.Instance.Return(this);
        }
    }

}