using UnityEngine;
using System;

namespace RabbitDog
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
