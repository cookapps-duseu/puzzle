using System;
using CookApps.UIManagements;
using CookApps.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CookApps.UIExtensions
{
    [Serializable]
    public class InstantParticle
    {
        [SerializeField] private AssetReferenceGameObject particleRef;

        private ParticleSystem particle;

        public async Awaitable Ready(Transform parent, bool autoPlay = false, Vector3 autoPlayPosition = default)
        {
            if (!particleRef.RuntimeKeyIsValid())
                return;

            if (parent == null)
                parent = SceneUILayerManager.Instance.FloatingNode;
            var handle = particleRef.InstantiateAsync(parent);
            await handle.WaitUntilDone();
            handle.Result.SetActive(false);
            particle = handle.Result.GetComponent<ParticleSystem>();
            var destroyNotifier = particle.gameObject.AddComponent<DestroyNotifier>();
            destroyNotifier.OnDestroyed += () =>
            {
                handle.Release();
                particle = null;
            };
            if (autoPlay)
                Play(autoPlayPosition);
        }
        
        public void Play()
        {
            Play(Vector2.zero);
        }
        
        public void Play(Vector3 position)
        {
            if (particle == null)
            {
                Debug.LogError("InstantParticle: Particle system not loaded. Call Ready() first.");
                return;
            }
            
            particle.transform.position = position;
            particle.gameObject.SetActive(true);
            particle.Play();
        }
    }
}
