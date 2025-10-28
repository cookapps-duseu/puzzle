using UnityEngine;
using System;

namespace CookApps
{
    public class DestroyNotifier : MonoBehaviour
    {
        public event Action OnDestroyed;

        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
            OnDestroyed = null;
        }
    }
}
