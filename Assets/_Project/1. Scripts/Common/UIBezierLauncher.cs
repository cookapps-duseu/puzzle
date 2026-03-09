using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CookApps.Inspector;
using CookApps.Utility;
using UnityEngine;

namespace Template
{
    public class UIBezierLauncher : MonoBehaviour
    {
        [Header("Movers to Launch (Assign your UIBezierMover components)")]
        private List<UIBezierMover> movers = new List<UIBezierMover>();

        [Header("Launch Settings")]
        [SerializeField, ReadOnly] private KeyCode repeatKey = KeyCode.K;
        public float interval = 0.2f;

        private bool isInitialized;
        public bool isLaunched;
        
        public event System.Action<int, int> OnLaunched;
        public event System.Action<int, int> OnArrived;
        public event System.Action OnDestroyed;

        private Action<int> CachedOnCompleted;
        
        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
            CachedOnCompleted = null;
        }

        private void Awake()
        {
            CachedOnCompleted = OnMoverCompleted;
        }
        
        public void AddMover(UIBezierMover mover)
        {
            if (isInitialized)
            {
                Debug.LogError("UIBezierLauncher: Cannot add mover after initialization.");
                return;
            }
            
            if (!movers.Contains(mover))
                movers.Add(mover);
        }

        public void Initialize(Transform dest = null)
        {
            if (isInitialized)
            {
                Debug.LogWarning("UIBezierLauncher: Already initialized.");
                return;
            }
            isInitialized = true;

            for (var i = 0; i < movers.Count; i++)
            {
                movers[i].Initialize(i, dest, CachedOnCompleted);
            }
        }

    #if UNITY_EDITOR
        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current == null)
                return;

            if (UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
                LaunchAsync().Forget();
        }
    #endif

        public async UniTask LaunchAsync()
        {
            if (!isInitialized)
            {
                Debug.LogError("UIBezierLauncher: Please call Initialize() before Launch().");
                return;
            }

            if (isLaunched)
            {
                Debug.LogWarning("UIBezierLauncher: Already launched.");
                return;
            }

            isLaunched = true;
            
            for (int i = 0; i < movers.Count; i++)
            {
                movers[i].StartMovement().Forget();
                OnLaunched?.Invoke(i, movers.Count);
                await UniTask.WaitForSeconds(interval, cancellationToken: destroyCancellationToken);
                if (destroyCancellationToken.IsCancellationRequested)
                    break;
            }
            
            isLaunched = false;
        }

        private void OnMoverCompleted(int index)
        {
            OnArrived?.Invoke(index, movers.Count);
        }
    }
}
