using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RabbitDog.UIExtensions
{
    public class LobbyPageController : CachedMonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private float percentThreshold = 0.2f;
        [SerializeField] private float animDuration = 0.2f;
        [SerializeField] private RectTransform content;
        [SerializeField] private LobbyPageBase[] pages;
        [SerializeField] private float buttonMoveDuration = 0.5f;
        [SerializeField] private LobbyPageBottomButton[] bottomButtons;
        [SerializeField] private RectTransform pageIndicator;

        private int activatePageIdx;
        public int ActivatePageIndex => activatePageIdx;
        private int pointerId;
        private Coroutine goToRoutine;
        
        public T GetPage<T>() where T : LobbyPageBase
        {
            for (var i = 0; i < pages.Length; i++)
            {
                LobbyPageBase panel = pages[i];
                if (panel is T page)
                {
                    return page;
                }
            }

            return null;
        }

        public void Initialize()
        {
            pointerId = -1;
            InitializePages();
            GoTo(pages.Length / 2, false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (LobbyPageBase page in pages)
            {
                page.OnControllerDestroyed();
            }
        }

        private void InitializePages()
        {
            var size = CachedRectTr.rect.size;
            size.x += 2;
            for (var i = 0; i < pages.Length; i++)
            {
                LobbyPageBase page = pages[i];
                page.Initialize(i, size);
            }

            Vector2 contentSize = content.sizeDelta;
            contentSize.x = pages.Length * size.x;
            content.sizeDelta = contentSize;
            float firstPageWidth = pages[0].CachedRectTr.rect.width;
            content.pivot = new Vector2(firstPageWidth * 0.5f / contentSize.x, 0.5f);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (goToRoutine != null)
            {
                StopCoroutine(goToRoutine);
                goToRoutine = null;
            }
            pointerId = eventData.pointerId;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != pointerId)
            {
                return;
            }

            float difference = eventData.pressPosition.x - eventData.position.x;
            Vector2 pos = content.anchoredPosition;
            pos.x = -pages[activatePageIdx].CachedRectTr.anchoredPosition.x - difference;
            if (pos.x > 0)
            {
                pos.x = 0;
            }
            if (pos.x < -(content.rect.size.x - pages[^1].CachedRectTr.rect.width))
            {
                pos.x = -(content.rect.size.x - pages[^1].CachedRectTr.rect.width);
            }
            content.anchoredPosition = pos;
            UpdatePages();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            float difference = eventData.pressPosition.x - eventData.position.x;
            float percentage = difference / CachedRectTr.rect.size.x;
            if (Mathf.Abs(percentage) >= percentThreshold)
            {
                int nextIndex = activatePageIdx + (int) Mathf.Sign(percentage);
                GoTo(nextIndex);
            }
            else
            {
                GoTo(activatePageIdx);
            }
        }

        public void GoTo(int targetIndex, bool withAnim = true)
        {
            targetIndex = Mathf.Clamp(targetIndex, 0, pages.Length - 1);

            activatePageIdx = targetIndex;
            if (goToRoutine != null)
            {
                StopCoroutine(goToRoutine);
                goToRoutine = null;
            }
            if (withAnim)
            {
                float targetX = -pages[activatePageIdx].CachedRectTr.anchoredPosition.x;
                goToRoutine = StartCoroutine(AnimateGoToX(targetX, animDuration));
            }
            else
            {
                Vector2 pos = content.anchoredPosition;
                pos.x = -pages[activatePageIdx].CachedRectTr.anchoredPosition.x;
                content.anchoredPosition = pos;
                UpdatePages();
                pages[activatePageIdx].OnGoTo();
            }

            for (var i = 0; i < bottomButtons.Length; i++)
            {
                bottomButtons[i].SetSelected(i == targetIndex);
            }
        }

        private void UpdatePages()
        {
            Vector2 contentPos = content.anchoredPosition;
            foreach (var page in pages)
            {
                page.OnDrag(contentPos, CachedRectTr.rect.size);
            }

            float contentPosRange = content.rect.width - pages[0].CachedRectTr.rect.width * 0.5f - pages[^1].CachedRectTr.rect.width * 0.5f;
            float xRatio = -contentPos.x / contentPosRange;
            pageIndicator.anchorMin = new Vector2(xRatio, 0f);
            pageIndicator.anchorMax = new Vector2(xRatio, 1f);
        }

        public void OnClickBottomMenu(int toggleIdx)
        {
            GoTo(toggleIdx);
        }

        private IEnumerator AnimateGoToX(float targetX, float duration)
        {
            Vector2 startPos = content.anchoredPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutCirc(t);

                float x = Mathf.Lerp(startPos.x, targetX, t);
                // Clamp within valid range similarly to drag
                x = ClampContentX(x);
                Vector2 pos = content.anchoredPosition;
                pos.x = x;
                content.anchoredPosition = pos;
                UpdatePages();
                yield return null;
            }

            // Finalize
            Vector2 finalPos = content.anchoredPosition;
            finalPos.x = ClampContentX(targetX);
            content.anchoredPosition = finalPos;
            UpdatePages();
            pages[activatePageIdx].OnGoTo();
            goToRoutine = null;
        }

        private float ClampContentX(float x)
        {
            float minX = -(content.rect.size.x - pages[^1].CachedRectTr.rect.width);
            if (x < minX) x = minX;
            if (x > 0f) x = 0f;
            return x;
        }

        private float EaseOutCirc(float t)
        {
            t = Mathf.Clamp01(t);
            return Mathf.Sqrt(1f - (t - 1f) * (t - 1f));
        }
    }
}
