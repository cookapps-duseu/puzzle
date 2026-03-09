using System;
using Cysharp.Threading.Tasks;
using CookApps.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CookApps.UIExtensions
{
    [Serializable]
    public class InstantParticle
    {
        [SerializeField] private AssetReferenceGameObject particleRef;

        public UniTask<ParticleSystem> Create(Transform parent)
        {
            return Create(parent, false);
        }

        public async UniTask<ParticleSystem> Create(Transform parent, bool autoPlay, bool isModifyPosition = false, Vector3 localPosition = default)
        {
            if (!particleRef.RuntimeKeyIsValid())
                return null;

            var handle = particleRef.InstantiateAsync(parent);
            await handle.WaitUntilDone();
            handle.Result.SetActive(false);
            var particle = handle.Result.GetComponent<ParticleSystem>();
            var destroyNotifier = particle.gameObject.AddComponent<DestroyNotifier>();
            destroyNotifier.OnDestroyed += () =>
            {
                handle.Release();
                particle = null;
            };

            if (isModifyPosition)
                particle.transform.localPosition = localPosition;

            if (autoPlay)
            {
                particle.gameObject.SetActive(true);
                particle.Play();
            }
            return particle;
        }
    }
}
