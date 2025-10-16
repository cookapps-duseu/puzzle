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
        bool TouchBegan(Vector3 touchPos);
        bool TouchMoved(Vector3 startTouchPos, Vector3 touchPos, Vector3 deltaPos);
        bool TouchEnded(Vector3 startTouchPos, Vector3 touchPos);
        void TouchCanceled();
    }
}