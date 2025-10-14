using System.Collections.Generic;
using CookApps.Iap.Result;
using MemoryPack;

namespace Template
{
    [MemoryPackable]
    public partial class UserShopHistory
    {
        public string productId;
        public int purchaseCount;
    }

    [MemoryPackable]
    public partial class UserShopData
    {
        public List<UserShopHistory> assets = new();
    }
    
    public sealed class UserShopDataContainer : UserDataContainerBase<UserShopData>
    {
        public override string PreferenceKey => "UserShopData";

        public override void InitData()
        {
            base.InitData();
            CookAppsIapWrapper.OnPurchaseSuccess += RecordPurchase;
        }
        
        public override void Dispose()
        {
            CookAppsIapWrapper.OnPurchaseSuccess -= RecordPurchase;
        }
        
        public void RecordPurchase(PurchaseResult result)
        {
            //AppsFlyer 결제 이벤트 호출
            // AppsflyerManager.Instance.SendPurchaseEvent(result);
            // if (data.assets.Count == 0)
            // {
            //     AppsflyerManager.Instance.SendFirstTimePurchaseEvent(result);
            // }
            
            var history = data.assets.Find(h => h.productId == result.ProductId);
            if (history != null)
            {
                history.purchaseCount++;
            }
            else
            {
                data.assets.Add(new UserShopHistory
                {
                    productId = result.ProductId,
                    purchaseCount = 1
                });
            }
            isDirty = true;
        }
        
        public int GetPurchaseCount(string productId)
        {
            var history = data.assets.Find(h => h.productId == productId);
            return history?.purchaseCount ?? 0;
        }
        
        public void ClearPurchaseHistory()
        {
            data.assets.Clear();
            isDirty = true;
        }
        
        public int GetTotalPurchaseCount()
        {
            int total = 0;
            foreach (var history in data.assets)
            {
                total += history.purchaseCount;
            }
            return total;
        }
        
        public decimal GetTotalPurchaseAmountUSD()
        {
            decimal total = 0;
            foreach (var history in data.assets)
            {
                var product = CookAppsIapWrapper.Instance.GetProduct(history.productId);
                if (product != null)
                {
                    total += product.metadata.localizedPrice * history.purchaseCount;
                }
            }
            return total;
        }
    }
}
