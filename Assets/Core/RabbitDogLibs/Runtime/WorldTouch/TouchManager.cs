using System.Collections.Generic;
using RabbitDog.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Pool;

namespace RabbitDog.WorldTouch
{
    public class TouchManager : SingletonMonoBehaviour<TouchManager>
    {
        private readonly List<ITouchBlocker> _blockers = new();
        private List<ITouchListener> managedListeners = new ();
        private List<ITouchListener> willAddListeners = new ();
        private List<ITouchListener> willRemoveListeners = new ();

        private int layerMask;

        private class TouchState
        {
            public int TouchId;
            public Vector2 StartPos;
            public Vector2 Pos;
            public Vector2 Delta;
            public ITouchListener Listener;
            public TouchCorrectionCylinder Cylinder;
        }

        private readonly Dictionary<int, TouchState> activeTouches = new ();
        private ObjectPool<TouchCorrectionCylinder> cylinderPool;
        public bool IsTouching => activeTouches.Count > 0;

        private Camera mainCamera;

        protected void Awake()
        {
            EnhancedTouchSupport.Enable();

            // Initialize cylinder pool
            cylinderPool = new ObjectPool<TouchCorrectionCylinder>(
                createFunc: () =>
                {
                    var inst = TouchCorrectionCylinder.Create(transform, 0.5f);
                    inst.CachedGo.SetActive(false);
                    return inst;
                },
                actionOnGet: c =>
                {
                    c.Clear();
                    c.CachedGo.SetActive(true);
                },
                actionOnRelease: c =>
                {
                    c.Clear();
                    c.CachedGo.SetActive(false);
                },
                actionOnDestroy: c =>
                {
                    if (c != null)
                        Destroy(c.CachedGo);
                },
                collectionCheck: false,
                defaultCapacity: 4,
                maxSize: 16
            );

            ObjectRegistry.Registered += OnRegistryRegistered;
            ObjectRegistry.Unregistered += OnRegistryUnregistered;
            RefreshMainCamera();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ObjectRegistry.Registered -= OnRegistryRegistered;
            ObjectRegistry.Unregistered -= OnRegistryUnregistered;
            
            // clear all listeners
            _blockers.Clear();
            managedListeners.Clear();
            willAddListeners.Clear();
            willRemoveListeners.Clear();
            
            CancelAllTouches();

            EnhancedTouchSupport.Disable();
            cylinderPool.Dispose();
            cylinderPool = null;
        }

        public int GetLayerMask()
        {
            return layerMask;
        }
        
        public void SetLayerMask(int mask1, int mask2 = -1, int mask3 = -1, int mask4 = -1, int mask5 = -1, int mask6 = -1, int mask7 = -1, int mask8 = -1)
        {
            layerMask = 0;
            if (mask1 != -1)
                layerMask |= 1 << mask1;
            if (mask2 != -1)
                layerMask |= 1 << mask2;
            if (mask3 != -1)
                layerMask |= 1 << mask3;
            if (mask4 != -1)
                layerMask |= 1 << mask4;
            if (mask5 != -1)
                layerMask |= 1 << mask5;
            if (mask6 != -1)
                layerMask |= 1 << mask6;
            if (mask7 != -1)
                layerMask |= 1 << mask7;
            if (mask8 != -1)
                layerMask |= 1 << mask8;
        }

        public static void AddTouchable(ITouchListener target)
        {
            if (!IsAlive())
                return;
            Instance.willAddListeners.Add(target);
        }

        public static void RemoveTouchable(ITouchListener target)
        {
            if (!IsAlive())
                return;
            Instance.willRemoveListeners.Add(target);
        }

        public static void AddBlocker(ITouchBlocker blocker)
        {
            if (!IsAlive())
                return;

            if (blocker == null)
                return;

            if (Instance._blockers.Contains(blocker))
                return;

            Instance._blockers.Add(blocker);
        }

        public static void RemoveBlocker(ITouchBlocker blocker)
        {
            if (!IsAlive())
                return;

            Instance._blockers.Remove(blocker);
        }

        public void CancelAllTouches()
        {
            if (activeTouches.Count == 0)
                return;

            foreach (var kv in activeTouches)
            {
                kv.Value.Listener?.TouchCanceled();
                cylinderPool.Release(kv.Value.Cylinder);
            }
            activeTouches.Clear();
        }

        private void SetCylinderPosition(TouchCorrectionCylinder cyl, Vector2 screenPos, Camera rayCamera)
        {
            var ray = rayCamera.ScreenPointToRay(screenPos);
            cyl.CachedTr.position = ray.origin;
            cyl.CachedTr.rotation = Quaternion.LookRotation(ray.direction);
        }

        private void Update()
        {
            HandleTouchInput();
            ApplyModifiedTouchListeners();
        }

        private void HandleTouchInput()
        {
            var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

            for (int i = 0; i < touches.Count; i++)
            {
                var t = touches[i];
                switch (t.phase)
                {
                    case UnityEngine.InputSystem.TouchPhase.Began:
                        TouchBegan(t);
                        break;
                    case UnityEngine.InputSystem.TouchPhase.Moved:
                        TouchMoved(t);
                        break;
                    case UnityEngine.InputSystem.TouchPhase.Stationary:
                        // Ignore; treat as no-op for now
                        break;
                    case UnityEngine.InputSystem.TouchPhase.Ended:
                    case UnityEngine.InputSystem.TouchPhase.Canceled:
                        TouchEnded(t);
                        break;
                }
            }
        }

        private bool IsAllowedByAllBlockers(string listenerName)
        {
            foreach (var blocker in _blockers)
            {
                if (!blocker.IsAllowListener(listenerName))
                {
                    return false;
                }
            }
            return true;
        }

        private void TouchBegan(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
        {
            var touchPos = touch.screenPosition;
            var deltaPos = Vector2.zero;
            var touchStartPos = touchPos;

            var uiTouch = EventSystem.current != null && IsPointerOverUIObject(touchPos);
            if (uiTouch)
            {
                return;
            }

            // Always allocate cylinder and state for this touch
            var cyl = cylinderPool.Get();
            cyl.SetHeight((transform.position - Vector3.zero).magnitude + 10f);
            var rayCamera = GetMainCamera();
            SetCylinderPosition(cyl, touchPos, rayCamera);
            var state = new TouchState
            {
                TouchId = touch.touchId,
                StartPos = touchStartPos,
                Pos = touchPos,
                Delta = deltaPos,
                Listener = null,
                Cylinder = cyl,
            };
            activeTouches[touch.touchId] = state;

            // Try to determine initial listener
            var ray = rayCamera.ScreenPointToRay(touchPos);
            if (!Physics.Raycast(ray, out var hit, rayCamera.transform.position.y + 10f, layerMask))
            {
                return;
            }

            ITouchListener hitListener;
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                hitListener = cyl.GetMostNearestObject();
            }
            else
            {
                hitListener = hit.transform.GetComponentInParent<ITouchListener>();
            }

            if (hitListener != null && IsAllowedByAllBlockers(hitListener.ListenerName))
            {
                state.Listener = hitListener;
                hitListener.TouchBegan(touchPos);
            }
        }

        private void TouchMoved(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
        {
            if (!activeTouches.TryGetValue(touch.touchId, out var state))
            {
                return;
            }

            var curPos = touch.screenPosition;
            var deltaPos = curPos - state.Pos;
            state.Delta = deltaPos;
            state.Pos = curPos;

            // Update cylinder position for this touch every frame
            var rayCamera = GetMainCamera();
            SetCylinderPosition(state.Cylinder, state.Pos, rayCamera);
            var ray = rayCamera.ScreenPointToRay(curPos);
            if (!Physics.Raycast(ray, out var hit, rayCamera.transform.position.y + 10f, layerMask))
            {
                state.Listener?.TouchCanceled();
                state.Listener = null;
                return;
            }

            ITouchListener hitListener;
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                if (state.Cylinder == null)
                {
                    state.Cylinder = cylinderPool.Get();
                    state.Cylinder.SetHeight((transform.position - Vector3.zero).magnitude + 10f);
                }
                SetCylinderPosition(state.Cylinder, state.Pos, rayCamera);
                hitListener = state.Cylinder.GetMostNearestObject();
                if (hitListener == null)
                {
                    state.Listener?.TouchCanceled();
                    state.Listener = null;
                    return;
                }
            }
            else
            {
                hitListener = hit.transform.GetComponentInParent<ITouchListener>();
                if (hitListener == null)
                {
                    state.Listener?.TouchCanceled();
                    state.Listener = null;
                    return;
                }
                // Keep cylinder alive for entire touch lifetime
            }

            if (IsAllowedByAllBlockers(hitListener.ListenerName))
            {
                if (state.Listener != hitListener)
                {
                    state.Listener?.TouchCanceled();
                    state.Listener = hitListener;
                }

                hitListener.TouchMoved(state.StartPos, state.Pos, state.Delta);
            }
        }

        private void TouchEnded(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
        {
            if (!activeTouches.TryGetValue(touch.touchId, out var state))
            {
                return;
            }

            Vector2 touchPos = touch.screenPosition;
            state.Pos = touchPos;
            state.Delta = Vector2.zero;

            // Final cylinder position for this touch
            var rayCamera = GetMainCamera();
            SetCylinderPosition(state.Cylinder, state.Pos, rayCamera);
            var ray = rayCamera.ScreenPointToRay(touchPos);
            if (!Physics.Raycast(ray, out var hit, rayCamera.transform.position.y + 10f, layerMask))
            {
                state.Listener?.TouchCanceled();
                state.Listener = null;
                cylinderPool.Release(state.Cylinder);
                state.Cylinder = null;
                activeTouches.Remove(touch.touchId);
                return;
            }

            ITouchListener hitListener;
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                hitListener = state.Cylinder.GetMostNearestObject();
                if (hitListener == null)
                {
                    state.Listener?.TouchCanceled();
                    state.Listener = null;
                    cylinderPool.Release(state.Cylinder);
                    state.Cylinder = null;
                    activeTouches.Remove(touch.touchId);
                    return;
                }
            }
            else
            {
                hitListener = hit.collider.GetComponentInParent<ITouchListener>();
                if (hitListener == null)
                {
                    state.Listener?.TouchCanceled();
                    state.Listener = null;
                    cylinderPool.Release(state.Cylinder);
                    state.Cylinder = null;
                    activeTouches.Remove(touch.touchId);
                    return;
                }
            }

            if (IsAllowedByAllBlockers(hitListener.ListenerName))
            {
                if (state.Listener != hitListener)
                {
                    state.Listener?.TouchCanceled();
                    state.Listener = null;
                }

                hitListener.TouchEnded(state.StartPos, state.Pos);
                foreach (var blocker in _blockers)
                {
                    blocker.OnClicked(hitListener.ListenerName);
                }
            }
            cylinderPool.Release(state.Cylinder);
            state.Cylinder = null;
            activeTouches.Remove(touch.touchId);
        }

        private void OnRegistryRegistered(RegistryKey key, IRegistrable registrable)
        {
            if (key == RegistryKey.MainCamera)
            {
                RefreshMainCamera();
            }
        }

        private void OnRegistryUnregistered(RegistryKey key, IRegistrable registrable)
        {
            if (key == RegistryKey.MainCamera)
            {
                RefreshMainCamera();
            }
        }

        private Camera GetMainCamera()
        {
            if (mainCamera == null)
            {
                RefreshMainCamera();
            }

            return mainCamera;
        }

        private void RefreshMainCamera()
        {
            mainCamera = null;
            if (ObjectRegistry.Instance.TryGetObject(RegistryKey.MainCamera, out var registeredObject))
            {
                mainCamera = registeredObject != null ? registeredObject.GetComponent<Camera>() : null;
            }
            if (mainCamera != null)
            {
                transform.position = mainCamera.transform.position;
            }
        }

        private void ApplyModifiedTouchListeners()
        {
            if (willAddListeners.Count == 0 && willRemoveListeners.Count == 0)
            {
                return;
            }

            for (var i = 0; i < willAddListeners.Count; i++)
            {
                managedListeners.Add(willAddListeners[i]);
            }

            willAddListeners.Clear();

            for (var i = 0; i < willRemoveListeners.Count; i++)
            {
                for (var j = 0; j < managedListeners.Count; j++)
                {
                    if (managedListeners[j] == willRemoveListeners[i])
                    {
                        managedListeners[j] = null;
                        break;
                    }
                }
            }

            willRemoveListeners.Clear();
            managedListeners.RemoveAll(x => x == null);
            managedListeners.Sort((x, y) => y.TouchPriority - x.TouchPriority);
        }

        private bool IsPointerOverUIObject(Vector2 touchPos)
        {
            var eventDataCurrentPosition = new PointerEventData(EventSystem.current)
            {
                position = touchPos,
            };
            using var _ = ListPool<RaycastResult>.Get(out var results);
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
    }
}
