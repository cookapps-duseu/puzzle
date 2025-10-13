using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RabbitDog
{
    public readonly struct LoadAssetHandleHolder<T> : IDisposable where T : UnityEngine.Object
    {
        private readonly bool isConstructed;
        private readonly AsyncOperationHandle<T> Handle;
        
        public LoadAssetHandleHolder(string address)
        {
            isConstructed = true;
            Handle = Addressables.LoadAssetAsync<T>(address);
        }
        
        public LoadAssetHandleHolder(AssetReferenceT<T> assetReference)
        {
            isConstructed = true;
            Handle = assetReference.LoadAssetAsync();
        }

        public AsyncOperationHandle<T> GetHandle()
        {
            return Handle;
        }

        private void ReleaseUnmanagedResources()
        {
            if (!isConstructed)
                return;
            Addressables.Release(Handle);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
        }
    }

    public readonly struct InstantiateHandleHolder : IDisposable
    {
        private readonly bool isConstructed;
        private readonly AsyncOperationHandle<GameObject> Handle;
        
        public InstantiateHandleHolder(string address, Transform parent)
        {
            isConstructed = true;
            Handle = Addressables.InstantiateAsync(address, parent);
        }
        
        public InstantiateHandleHolder(AssetReferenceT<GameObject> assetRef, Transform parent)
        {
            isConstructed = true;
            Handle = Addressables.InstantiateAsync(assetRef, parent);
        }

        public AsyncOperationHandle<GameObject> GetHandle()
        {
            return Handle;
        }
        
        private void ReleaseUnmanagedResources()
        {
            if (!isConstructed)
                return;
            var go = Handle.Result;
            Addressables.ReleaseInstance(Handle);
            UnityEngine.Object.Destroy(go);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
        }
    }
}