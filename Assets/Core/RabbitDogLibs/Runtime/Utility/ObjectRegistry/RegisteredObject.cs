using UnityEngine;

namespace RabbitDog.Utility
{
    public class RegisteredObject : CachedMonoBehaviour, IRegistrable
    {
        [SerializeField] private RegistryKey registryKey;
 
        public RegistryKey Key => registryKey;
        
        private void Awake()
        {
            ObjectRegistry.Instance.Register(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ObjectRegistry.Instance.Unregister(this);
        }
    }
}
