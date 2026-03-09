using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CookApps.Utility
{
    public static class GameObjectPoolManager
    {
        private static Dictionary<string, GameObjectPool> pools = new ();

        public static void Clear()
        {
            var keys = new List<string>(pools.Keys);
            foreach (var key in keys)
            {
                ReleasePool(key).Forget();
            }
        }

        public static async UniTask PreloadPool<T>(string poolPrefabAddress) where T : Component
        {
            if (pools.TryGetValue(poolPrefabAddress, out GameObjectPool pool))
            {
                await pool.WaitForInitialize();
                if (pool is GameObjectPool<T> typedPool)
                {
                    return;
                }

                pool.ReleasePool();
                pools.Remove(poolPrefabAddress);
            }

            var handle = Addressables.LoadAssetAsync<GameObject>(poolPrefabAddress);
            var objectPool = new GameObjectPool<T>();
            pools.Add(poolPrefabAddress, objectPool);
            await handle.WaitUntilDone();
            objectPool.Initialize(handle);
        }

        public static async UniTask<GameObjectPool<T>> GetPoolAsync<T>(string poolPrefabAddress) where T : Component
        {
            if (pools.TryGetValue(poolPrefabAddress, out GameObjectPool pool))
            {
                await pool.WaitForInitialize();
                if (pool is GameObjectPool<T> typedPool)
                {
                    return typedPool;
                }

                pool.ReleasePool();
                pools.Remove(poolPrefabAddress);
            }

            var handle = Addressables.LoadAssetAsync<GameObject>(poolPrefabAddress);
            var objectPool = new GameObjectPool<T>();
            pools.Add(poolPrefabAddress, objectPool);
            objectPool.Initialize(handle);
            await handle.WaitUntilDone();
            if (!handle.IsValid())
                return null;
            return objectPool;
        }

        public static GameObjectPool<T> GetPool<T>(string poolPrefabAddress) where T : Component
        {
            if (pools.TryGetValue(poolPrefabAddress, out GameObjectPool pool))
            {
                if (pool is GameObjectPool<T> typedPool)
                {
                    return typedPool;
                }

                pool.ReleasePool();
                pools.Remove(poolPrefabAddress);
            }

            throw new Exception($"No pool found for type {typeof(T)} with pool type {poolPrefabAddress}");
        }

        public static async UniTask ReleasePool(string poolType)
        {
            if (pools.Remove(poolType, out GameObjectPool pool))
            {
                await pool.WaitForInitialize();
                pool.ReleasePool();
            }
        }

        public static async UniTask ReleasePool(GameObjectPool pool)
        {
            foreach (var kvp in pools)
            {
                if (kvp.Value == pool)
                {
                    pools.Remove(kvp.Key);
                    break;
                }
            }
            await pool.WaitForInitialize();
            pool.ReleasePool();
        }
    }
}
