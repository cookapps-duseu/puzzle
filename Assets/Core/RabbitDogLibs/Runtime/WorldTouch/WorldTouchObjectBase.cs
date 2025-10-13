using RabbitDog.UIManagements;
using UnityEngine;

namespace RabbitDog.WorldTouch
{
    public abstract class WorldTouchObjectBase : CachedMonoBehaviour, ITouchListener
    {
        protected virtual void OnEnable()
        {
            TouchManager.AddTouchable(this);
        }
        
        protected virtual void OnDisable()
        {
            TouchManager.RemoveTouchable(this);
        }
        
        protected abstract bool OnTouchObject();

        public string ListenerName => name;

        public int TouchPriority => 0;

        private bool isTouched = false;

        public void TouchBegan(RaycastHit2D[] hits, int hitCount, Vector3 touchPos)
        {
            for (var i = 0; i < hitCount; i++)
            {
                if (hits[i].collider.gameObject == CachedGo)
                {
                    isTouched = true;
                }
            }
        }

        public bool TouchMoved(RaycastHit2D[] hits, int hitCount, Vector3 startTouchPos, Vector3 touchPos, Vector3 deltaPos)
        {
            if (!isTouched)
                return false;

            if (UIManagementsConst.Default.DragThreshold < Vector3.Distance(startTouchPos, touchPos))
                isTouched = false;

            return false;
        }

        public bool TouchEnded(RaycastHit2D[] hits, int hitCount, Vector3 startTouchPos, Vector3 touchPos)
        {
            if (isTouched)
            {
                var res = OnTouchObject();
                isTouched = false;
                return res;
            }

            return false;
        }

        public void TouchCanceled()
        {
            isTouched = false;
        }
    }
}