using RabbitDog;
using RabbitDog.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Template
{
    public class ShopProductNode : CachedMonoBehaviour
    {
        private AsyncOperationHandle<GameObject> loadedHandle;
        private ISpecShop spec;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (loadedHandle.IsValid())
            {
                loadedHandle.Release();
            }
        }

        public void UpdateUI(ISpecShop spec)
        {
            this.spec = spec;
            LoadShopPrefab(spec).Forget();
        }

        private async Awaitable LoadShopPrefab(ISpecShop spec)
        {
            if (loadedHandle.IsValid())
            {
                loadedHandle.Release();
            }
            
            var handle = Addressables.InstantiateAsync($"Prefabs/UI/ShopPackages/Shop_PackageSlot_{spec.Id}.prefab", CachedTr);
            loadedHandle = handle;
            await handle.WaitUntilDone();

            if (!handle.IsValid())
                return;

            var product = handle.Result.GetComponent<ShopProduct>();
            product.UpdateUI(spec);
        }
    }
}
