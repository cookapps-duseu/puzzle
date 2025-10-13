using UnityEngine;
using UnityEngine.UI;

namespace RabbitDog.Utility
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaBottomReverseMargin : MonoBehaviour
    {
        private static float? marginBottom;
        public float MarginBottom => marginBottom ?? 0;

        private void Start()
        {
            var rectTr = GetComponent<RectTransform>();
            var originSizeDelta = rectTr.sizeDelta;
            var originAnchoredPosition = rectTr.anchoredPosition;
            var canvasScaler = GetComponentInParent<CanvasScaler>();
            var canvasScalerRectTr = canvasScaler.GetComponent<RectTransform>();

            if (marginBottom == null)
            {
                var safeArea = Screen.safeArea;
                var resolution = Screen.fullScreen ? new Vector2(Screen.currentResolution.width, Screen.currentResolution.height) : new Vector2(Screen.width, Screen.height);

                marginBottom = safeArea.y;
                float resolutionRatio;
                if (Mathf.Approximately(canvasScalerRectTr.rect.size.x, canvasScaler.referenceResolution.x))
                {
                    resolutionRatio = canvasScaler.referenceResolution.x / resolution.x;
                }
                else
                {
                    resolutionRatio = canvasScaler.referenceResolution.y / resolution.y;
                }

                marginBottom = marginBottom * resolutionRatio * SafeArea.MarginRatio.bottom;
            }
            rectTr.sizeDelta = originSizeDelta + new Vector2(0f, MarginBottom);
            rectTr.anchoredPosition = originAnchoredPosition + new Vector2(0f, MarginBottom * (rectTr.pivot.y - 1f));
        }
    }
}
