using CookApps;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CookApps.UIExtensions
{
    public class LobbyPageBottomButton : CachedMonoBehaviour
    {
        [SerializeField] private SimpleSwapper[] swappers;
        [SerializeField] private Transform icon;
        [SerializeField] private LayoutElement layoutElement;
        
        private bool isInit = false;
        private bool isSelected = false;
        private Coroutine animateRoutine;
        
        public void SetSelected(bool isSelected)
        {
            if (isInit && this.isSelected == isSelected)
                    return;
            isInit = true;
            this.isSelected = isSelected;
            swappers.Swap(isSelected ? SimpleSwapType.Normal : SimpleSwapType.Disabled);
            var targetScale = 1f;
            var prevScale = 1.2f;
            if (isSelected)
            {
                targetScale = 1.2f;
                prevScale = 1f;
            }

            if (animateRoutine != null)
            {
                StopCoroutine(animateRoutine);
                animateRoutine = null;
            }
            animateRoutine = StartCoroutine(AnimateSelection(prevScale, targetScale, 0.1f));
        }

        private IEnumerator AnimateSelection(float fromScale, float toScale, float duration)
        {
            float elapsed = 0f;

            icon.localScale = Vector3.one * fromScale;
            layoutElement.flexibleWidth = fromScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutCirc(t);

                float scale = Mathf.Lerp(fromScale, toScale, t);
                icon.localScale = Vector3.one * scale;
                layoutElement.flexibleWidth = scale;
                yield return null;
            }

            icon.localScale = Vector3.one * toScale;
            layoutElement.flexibleWidth = toScale;

            animateRoutine = null;
        }

        private float EaseOutCirc(float t)
        {
            t = Mathf.Clamp01(t);
            return Mathf.Sqrt(1f - Mathf.Clamp01((t - 1f) * (t - 1f)));
        }
    }
}
