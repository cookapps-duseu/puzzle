using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CookApps.Utility
{
    public abstract class GameObjectPool
    {
        protected virtual bool IsInitialized { get; }
        public virtual async Awaitable WaitForInitialize()
        {
            while (!IsInitialized)
                await Awaitable.NextFrameAsync();
        }

        public abstract bool ReleasePool();
    }

    public class GameObjectPool<T> : GameObjectPool where T : Component
    {
        private AsyncOperationHandle<GameObject> handle;
        private IObjectPool<T> pool;

        protected override bool IsInitialized => initializeCount > 0;
        private int initializeCount = 0;
        private int maxCapacity = int.MaxValue / 2;
        private int currentCapacity = 0;

        public void Initialize(AsyncOperationHandle<GameObject> handle, int maxCapacity = int.MaxValue / 2)
        {
            initializeCount++;
            if (initializeCount > 1)
            {
                return;
            }
            if (maxCapacity <= 0)
                maxCapacity = int.MaxValue / 2;
            this.maxCapacity = maxCapacity;
            currentCapacity = this.maxCapacity;
            this.handle = handle;
            pool = new LinkedPool<T>(CreatePooledItem, null, OnReturnedToPool, OnDestroyPoolObject);
        }

        public override bool ReleasePool()
        {
            initializeCount--;
            if (initializeCount > 0)
            {
                return false;
            }
            pool?.Clear();
            pool = null;
            maxCapacity = int.MaxValue / 2;
            currentCapacity = 0;
            handle.Release();
            return true;
        }

        private T CreatePooledItem()
        {
            var go = Object.Instantiate(handle.Result);
            return go.GetComponent<T>();
        }

        private void OnReturnedToPool(T obj)
        {
            if (obj == null)
                return;

            obj.gameObject.SetActive(false);
            obj.transform.SetParent(GameObjectPoolTransformProvider.Instance.PoolTr, false);
        }

        private void OnDestroyPoolObject(T obj)
        {
            if (!Application.isPlaying)
                return;
            Object.Destroy(obj.gameObject);
        }

        public T Get(Transform parent)
        {
            if (initializeCount <= 0)
                return null;
            if (currentCapacity <= 0)
                return null;
            if (pool == null)
                return null;

            currentCapacity--;
            var poolObj = pool.Get();
            if (ReferenceEquals(parent, null))
            {
                Scene activeScene = SceneManager.GetActiveScene();
                poolObj.transform.SetParent(null);
                SceneManager.MoveGameObjectToScene(poolObj.gameObject, activeScene);
            }
            else
            {
                poolObj.transform.SetParent(parent, false);
            }
            poolObj.gameObject.SetActive(true);
            return poolObj;
        }

        public void Return(T poolObj)
        {
            if (initializeCount <= 0)
            {
                Object.Destroy(poolObj.gameObject);
                return;
            }

            currentCapacity++;
            if (maxCapacity < currentCapacity)
            {
                currentCapacity = maxCapacity;
                Object.Destroy(poolObj.gameObject);
                return;
            }
            pool.Release(poolObj);
        }
    }
}
