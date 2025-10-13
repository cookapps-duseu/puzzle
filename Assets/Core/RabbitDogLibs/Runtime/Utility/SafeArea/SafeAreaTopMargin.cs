using UnityEngine;
using UnityEngine.UI;

namespace RabbitDog.Utility
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaTopMargin : MonoBehaviour
    {
        [SerializeField] private bool extend;

        private static float? marginTop;
        public static float MarginTop => marginTop ?? 0;

        public bool IsExtend => extend;

        private void Start()
        {
            var rectTr = GetComponent<RectTransform>();
            var originSizeDelta = rectTr.sizeDelta;
            var originAnchoredPosition = rectTr.anchoredPosition;
            var canvasScaler = GetComponentInParent<CanvasScaler>();
            var canvasScalerRectTr = canvasScaler.GetComponent<RectTransform>();

            if (marginTop == null)
            {
                var safeArea = Screen.safeArea;
                var resolution = Screen.fullScreen ? new Vector2(Screen.currentResolution.width, Screen.currentResolution.height) : new Vector2(Screen.width, Screen.height);

                marginTop = resolution.y - (safeArea.y + safeArea.height);
                float resolutionRatio;
                if (Mathf.Approximately(canvasScalerRectTr.rect.size.x, canvasScaler.referenceResolution.x))
                {
                    resolutionRatio = canvasScaler.referenceResolution.x / resolution.x;
                }
                else
                {
                    resolutionRatio = canvasScaler.referenceResolution.y / resolution.y;
                }

                marginTop = marginTop * resolutionRatio * SafeArea.MarginRatio.top;
            }

            // WARNING! scale이 변경되었을 경우(by self or parent) 로직 수정 필요
            if (extend)
            {
                rectTr.sizeDelta = originSizeDelta + new Vector2(0f, MarginTop);
                rectTr.anchoredPosition = originAnchoredPosition - new Vector2(0f, MarginTop * (1f - rectTr.pivot.y));
            }
            else
            {
                rectTr.anchoredPosition = originAnchoredPosition - new Vector2(0f, MarginTop);
            }
        }
    }
}
