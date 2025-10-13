using UnityEngine;

namespace RabbitDog.UIExtensions
{
    public abstract class LobbyPageBase : CachedMonoBehaviour
    {
        private bool isEntered;

        public virtual void Initialize(int index, Vector2 safeAreaSize)
        {
            Vector2 size = CachedRectTr.sizeDelta;
            size.x = safeAreaSize.x;
            CachedRectTr.sizeDelta = size;
            CachedRectTr.anchoredPosition = new Vector2(index * safeAreaSize.x, CachedRectTr.anchoredPosition.y);
            gameObject.SetActive(false);
        }

        public void OnDrag(Vector2 pos, Vector2 size)
        {
            float width = CachedRectTr.rect.width;
            int rectMinX = Mathf.RoundToInt(CachedRectTr.anchoredPosition.x - (width * 0.5f));
            int rectMaxX = Mathf.RoundToInt(rectMinX + width);
            int viewPortMinX = Mathf.RoundToInt(-pos.x - (size.x * 0.5f));
            int viewPortMaxX = Mathf.RoundToInt(viewPortMinX + size.x);

            if (!isEntered && viewPortMinX < rectMaxX && rectMinX < viewPortMaxX)
            {
                gameObject.SetActive(true);
                isEntered = true;
                OnEnter();
            }

            if (isEntered && (viewPortMaxX <= rectMinX || rectMaxX <= viewPortMinX))
            {
                gameObject.SetActive(false);
                isEntered = false;
                OnExit();
            }
        }
        
        public void OnControllerDestroyed()
        {
            if (!isEntered)
                return;
            OnExit();
        }
        
        /// 스크린에 조금이라도 보이게 되었을 때
        protected virtual void OnEnter()
        {
        }
        
        /// 스크린에서 완전히 안보이게 되었을 때
        protected virtual void OnExit()
        {
        }
        
        /// 스크린 중앙에 도착하였을 때
        public virtual void OnGoTo()
        {
            
        }
    }
}
