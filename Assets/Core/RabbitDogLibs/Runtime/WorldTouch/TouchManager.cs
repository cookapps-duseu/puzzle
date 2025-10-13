using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace RabbitDog.WorldTouch
{
    public class TouchManager : SingletonMonoBehaviour<TouchManager>
    {
        private List<ITouchBlocker> blockers = new ();
        private List<ITouchListener> touchListeners = new List<ITouchListener>();
        private List<ITouchListener> willAddTouchListeners = new List<ITouchListener>();
        private List<ITouchListener> willRemoveTouchListeners = new List<ITouchListener>();
        private int layerMask;
        private readonly RaycastHit2D[] hits = new RaycastHit2D[8];

        private void Awake()
        {
            layerMask = 1 << LayerMask.NameToLayer("Touchable");
        }

        public static void AddTouchable(ITouchListener target)
        {
            if (Instance != null)
            {
                Instance.willAddTouchListeners.Add(target);
            }
        }

        public static void RemoveTouchable(ITouchListener target)
        {
            if (Instance != null)
            {
                Instance.willRemoveTouchListeners.Add(target);
            }
        }

        public static void AddBlocker(ITouchBlocker blocker)
        {
            if (Instance != null)
            {
                Instance.blockers.Add(blocker);
            }
        }

        public static void RemoveBlocker(ITouchBlocker blocker)
        {
            if (Instance != null)
            {
                Instance.blockers.Remove(blocker);
            }
        }

        public void CancelAllTouches()
        {
            if (isTouching)
            {
                int count = Instance.touchListeners.Count;
                for (int i = 0; i < count; i++)
                {
                    touchListeners[i].TouchCanceled();
                }

                isTouching = false;
            }
        }

        // Update is called once per frame
        private void Update()
        {
            TouchBegan();
            TouchMoved();
            TouchEnded();
            ApplyModifiedTouch();
        }

        private Vector3 touchStartPos;
        private Vector3 touchPos;
        private Vector3 deltaPos;
        private bool isTouching = false;

        private void TouchBegan()
        {
            bool ui_touch = false;
    #if UNITY_EDITOR
            if (!Input.GetMouseButtonDown(0))
                return;
            deltaPos = Vector3.zero;
            touchPos = Input.mousePosition;
            touchStartPos = touchPos;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                ui_touch = true;
    #else
            if (Input.touchCount != 1 || Input.GetTouch(0).phase != TouchPhase.Began)
                return;
            deltaPos = Vector3.zero;
            touchPos = Input.GetTouch(0).position;
            touchStartPos = touchPos;

            if (EventSystem.current != null)
            {
                if (IsPointerOverUIObject(touchPos))
                    ui_touch = true;
            }
    #endif
            if (ui_touch)
                return;

            isTouching = true;

            var ray = CameraManager.Main.ScreenPointToRay(touchPos);
            var hitCount = Physics2D.RaycastNonAlloc(ray.origin, ray.direction, hits, 50f, layerMask);
            for (var i = 0; i < touchListeners.Count; i++)
            {
                var touchListener = touchListeners[i];

                bool isBlocked = false;
                foreach (var blocker in blockers)
                {
                    if (!blocker.IsAllowListener(touchListener.ListenerName))
                    {
                        isBlocked = true;
                        break;
                    }
                }

                if (isBlocked)
                    continue;

                if (hitCount != 0)
                    touchListener.TouchBegan(hits, hitCount, touchPos);
            }
        }

        private void TouchMoved()
        {
            if (!isTouching) return;
    #if UNITY_EDITOR
            if (!Input.GetMouseButton(0))
                return;
            if (Input.GetAxis("Mouse X") == 0 && Input.GetAxis("Mouse Y") == 0)
                return;
            Vector3 curPos = Input.mousePosition;
            deltaPos = curPos - touchPos;
            touchPos = curPos;
    #else
            if (Input.touchCount != 1 || Input.GetTouch(0).phase != TouchPhase.Moved)
                return;

            Vector3 curPos = Input.GetTouch(0).position;
            deltaPos = curPos - touchPos;
            touchPos = curPos;
    #endif

            var ray = CameraManager.Main.ScreenPointToRay(touchPos);
            var hitCount = Physics2D.RaycastNonAlloc(ray.origin, ray.direction, hits, 50f, layerMask);
            for (var i = 0; i < touchListeners.Count; i++)
            {
                var touchListener = touchListeners[i];
                bool isBlocked = false;
                foreach (var blocker in blockers)
                {
                    if (!blocker.IsAllowListener(touchListener.ListenerName))
                    {
                        isBlocked = true;
                        break;
                    }
                }
                if (isBlocked)
                    continue;

                if (touchListener.TouchMoved(hits, hitCount, touchStartPos, touchPos, deltaPos))
                    break;
            }
        }

        private void TouchEnded()
        {
            if (!isTouching) return;
    #if UNITY_EDITOR
            if (!Input.GetMouseButtonUp(0))
                return;
            touchPos = Input.mousePosition;
            deltaPos = Vector3.zero;
    #else
            if (Input.touchCount != 1 || Input.GetTouch(0).phase != TouchPhase.Ended)
                return;
            touchPos = Input.GetTouch(0).position;
            deltaPos = Vector3.zero;
    #endif
            isTouching = false;

            var ray = CameraManager.Main.ScreenPointToRay(touchPos);
            var hitCount = Physics2D.RaycastNonAlloc(ray.origin, ray.direction, hits, 50f, layerMask);
            var isSwallowed = false;
            for (var i = 0; i < touchListeners.Count; i++)
            {
                var touchListener = touchListeners[i];
                bool isBlocked = false;
                foreach (var blocker in blockers)
                {
                    if (!blocker.IsAllowListener(touchListener.ListenerName))
                    {
                        isBlocked = true;
                        break;
                    }
                }
                if (isBlocked)
                    continue;
                
                if (isSwallowed)
                {
                    touchListener.TouchCanceled();
                }
                else
                {
                    isSwallowed = touchListener.TouchEnded(hits, hitCount, touchStartPos, touchPos);
                    if (isSwallowed)
                    {
                        foreach (var blocker in blockers)
                        {
                            blocker.OnClicked(touchListener.ListenerName);
                        }
                    }
                }
            }
        }

        private void ApplyModifiedTouch()
        {
            if (willAddTouchListeners.Count == 0 && willRemoveTouchListeners.Count == 0)
                return;

            for (int i = 0; i < willAddTouchListeners.Count; i++)
            {
                touchListeners.Add(willAddTouchListeners[i]);
            }

            willAddTouchListeners.Clear();

            for (int i = 0; i < willRemoveTouchListeners.Count; i++)
            {
                for (int j = 0; j < touchListeners.Count; j++)
                {
                    if (touchListeners[j] == willRemoveTouchListeners[i])
                    {
                        touchListeners[j] = null;
                        break;
                    }
                }
            }

            willRemoveTouchListeners.Clear();
            touchListeners.RemoveAll(x => x is null);
            touchListeners.Sort((x, y) => y.TouchPriority - x.TouchPriority);
        }

        private bool IsPointerOverUIObject(Vector2 touchPos)
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = touchPos;
            using var _ = ListPool<RaycastResult>.Get(out var results);
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
    }
}