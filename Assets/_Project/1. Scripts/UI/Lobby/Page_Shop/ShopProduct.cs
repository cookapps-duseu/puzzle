using CookApps.Iap.Result;
using RabbitDog;
using RabbitDog.Utility;
using UnityEngine;

namespace Template
{
    public class ShopProduct : CachedMonoBehaviour
    {
        [SerializeField] private StorePriceText priceText;
        
        private ISpecShop spec;
        
        public void UpdateUI(ISpecShop spec)
        {
            this.spec = spec;
            priceText.SetSpecData(spec);
        }

        public void OnClick()
        {
            Purchase().Forget();
        }

        private async Awaitable Purchase()
        {
            (bool isSuccess, PurchaseResult result) = await CookAppsIapWrapper.Instance.PurchaseProduct(spec.ProductId, spec.Price);
            if (isSuccess)
            {
                for (var i = 0; i < spec.RewardGroups.Rewards.Count; i++)
                {
                    var assetData = UserDataManager.Instance.GetAssetData();
                    assetData.AddReward(spec.RewardGroups.Rewards[i]);
                }
            }
            
            // TODO: 로그 남기기
        }
    }
}
