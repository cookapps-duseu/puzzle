using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CookApps.Iap;
using CookApps.Iap.Result;
using Newtonsoft.Json.Linq;
using CookApps;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Template
{
    public class CookAppsIapWrapper : Singleton<CookAppsIapWrapper>
    {
        private enum InitializedState
        {
            Success,
            OnlyUnityServices,
            AllFail,
        }

        private InitializedState initializedState = InitializedState.AllFail;
        private List<PurchaseResult> pendingResults = new ();

        public IReadOnlyList<PurchaseResult> GetPendingResultsSnapshot()
        {
            return pendingResults.ToArray();
        }

        public bool IsInitialized => initializedState == InitializedState.Success;
        public static event Action OnStoreInitialized;
        public static event Action<string> OnPurchaseStart;
        public static event Action<PurchaseResult> OnPurchaseSuccess;
        public static event Action<string> OnPurchaseFail;

        public async UniTask<bool> Initialize(List<ProductCatalogItem> items, Action<float> progressCallback)
        {
            //CookAppsIap 초기화
            IapInitializeParam param;
            if ((items?.Count ?? 0) > 0)
            {
                param = new IapInitializeParam(new CookAppsIapGeneral(), PurchaseProcessingResult.Pending, items, OnPendingPurchase);
            }
            else
            {
                param = new IapInitializeParam(new CookAppsIapGeneral(), PurchaseProcessingResult.Pending, OnPendingPurchase);
            }

            InitializeResult result = await CookAppsIap.Initialize(param);
            progressCallback?.Invoke(1f);
    #if UNITY_EDITOR
            CookAppsIap.UseFakeStoreAlways = true;
            CookAppsIap.UseFakeStoreUIMode = FakeStoreUIMode.StandardUser;
    #endif

            initializedState = result.Result == EnumResult.SUCCESS ? InitializedState.Success : InitializedState.OnlyUnityServices;
            if (initializedState == InitializedState.Success)
            {
                OnStoreInitialized?.Invoke();
            }

            return result.Result == EnumResult.SUCCESS;
        }

        private void OnPendingPurchase(PurchaseResult purchaseResult)
        {
            pendingResults.Add(purchaseResult);
        }

        public Product GetProduct(string productId)
        {
            if (!IsInitialized)
            {
                return null;
            }

            return CookAppsIap.Instance.GetProduct(productId);
        }

        public string GetPriceString(string productId)
        {
            if (!IsInitialized)
            {
                return null;
            }

            return CookAppsIap.Instance.GetPriceString(productId);
        }

        /// <summary>
        /// 리턴 받고 로직 처리 후 꼭 ConfirmPendingPurchase 호출 하세요.
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="dollarPrice"></param>
        /// <returns></returns>
        public async UniTask<(bool, PurchaseResult)> PurchaseProduct(string productId, float dollarPrice)
        {
            PurchaseResult result = await CookAppsIap.Instance.Purchase(productId);
            if (result.Result != EnumResult.SUCCESS)
            {
                return (false, result);
            }

            bool isVerified = await VerifyReceiptAsync(result, dollarPrice);
            if (!isVerified)
            {
                // await SceneUIManager.Instance.RequestPushUIAsync("Popup_ShopPurchaseFail");
                return (false, result);
            }

            Success(GetProduct(productId));
            OnPurchaseSuccess?.Invoke(result);
            return (true, result);
        }

        private void Success(Product product)
        {
            string order_id = product.transactionID;
            string product_id = product.definition.id;
            string currency_code = product.metadata.isoCurrencyCode;
            var currency_price = (double) product.metadata.localizedPrice;
            string receipt = product.receipt;

            // CookApps.AnalyticsLite.CAppEventLite.ReportInAppPurchase(order_id,
            //     product_id,
            //     currency_code,
            //     currency_price,
            //     receipt);
        }

        public async UniTask<bool> CheckReceiptAsync(PurchaseResult result, float dollarPrice)
        {
    #if UNITY_EDITOR
            // return true;
    #endif
            string orderId = GetOrderIdFromReceipt(result.Receipt);
            return await VerifyReceiptAsync(result, dollarPrice);
        }

        public static async UniTask<bool> VerifyReceiptAsync(PurchaseResult result, float dollarPrice)
        {
            var validPurchase = true; // Presume valid for platforms with no R.V.

            // Unity IAP's validation logic is only included on these platforms.
    #if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
            // Prepare the validator with the secrets we prepared in the Editor
            // obfuscation window.
            // var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
            //     AppleTangle.Data(), Application.bundleIdentifier);
            //
            // try
            // {
            //     // On Google Play, result has a single product ID.
            //     // On Apple stores, receipts contain multiple products.
            //     IPurchaseReceipt[] res = validator.Validate(result.Receipt);
            //     // For informational purposes, we list the receipt(s)
            //     Debug.Log("Receipt is valid. Contents:");
            //     foreach (IPurchaseReceipt productReceipt in res)
            //     {
            //         Debug.Log(productReceipt.productID);
            //         Debug.Log(productReceipt.purchaseDate);
            //         Debug.Log(productReceipt.transactionID);
            //     }
            // }
            // catch (IAPSecurityException)
            // {
            //     Debug.Log("Invalid receipt, not unlocking content");
            //     validPurchase = false;
            // }
    #endif

            return validPurchase;
        }

        public async UniTask ConfirmPendingPurchase(PurchaseResult purchaseResult)
        {
            var isFinished = false;
            var isSuccess = false;
            CookAppsIap.Instance.ConfirmPendingPurchase(purchaseResult.ProductId, res =>
            {
                isFinished = true;
                isSuccess = res;
            });

            while (!isFinished)
                await UniTask.NextFrame();
            if (!isSuccess)
            {
                return;
            }

            // var specShop = SpecDataManager.Instance.GetShopSpecWithProductId(purchaseResult.ProductId);
            // await VerifyReceiptAsync(purchaseResult, specShop.cash_price);

            for (var i = 0; i < pendingResults.Count; i++)
            {
                if (pendingResults[i].ProductId == purchaseResult.ProductId)
                {
                    pendingResults.RemoveAt(i);
                    break;
                }
            }
        }

        private static string GetOrderIdFromReceipt(string receipt)
        {
    #if UNITY_EDITOR
            JObject receiptJson = JObject.Parse(receipt);
            if (!receiptJson.TryGetValue("TransactionID", out JToken transactionIdToken))
            {
                return null;
            }

            var transactionId = transactionIdToken.Value<string>();
            return transactionId;
    #elif UNITY_ANDROID
            var receiptJson = JObject.Parse(receipt);
            if (!receiptJson.TryGetValue("Payload", out var payloadToken))
                return null;

            var payloadStr = payloadToken.Value<string>();
            if (payloadStr == null)
                return null;

            var payloadJson = JObject.Parse(payloadStr);
            if (!payloadJson.TryGetValue("json", out var realDataToken))
                return null;

            var realDataStr = realDataToken.Value<string>();
            if (realDataStr == null)
                return null;

            var realDataJson = JObject.Parse(realDataStr);
            if (!realDataJson.TryGetValue("orderId", out var orderIdToken))
                return null;

            return orderIdToken.Value<string>();
    #elif UNITY_IOS
            var receiptJson = JObject.Parse(receipt);
            if (!receiptJson.TryGetValue("TransactionID", out var transactionIdToken))
                return null;
            var transactionId = transactionIdToken.Value<string>();
            return transactionId;
    #else
            return null;
    #endif
        }
    }
}
