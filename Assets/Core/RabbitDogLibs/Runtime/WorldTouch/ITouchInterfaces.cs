using UnityEngine;

namespace RabbitDog.WorldTouch
{
    public interface ITouchBlocker
    {
        bool IsAllowListener(string listenerName);
        void OnClicked(string buttonName);
    }

    public interface ITouchListener
    {
        string ListenerName { get; }
        int TouchPriority { get; }
        void TouchBegan(RaycastHit2D[] hits, int hitCount, Vector3 touchPos);
        bool TouchMoved(RaycastHit2D[] hits, int hitCount, Vector3 startTouchPos, Vector3 touchPos, Vector3 deltaPos);
        bool TouchEnded(RaycastHit2D[] hits, int hitCount, Vector3 startTouchPos, Vector3 touchPos);
        void TouchCanceled();
    }
}