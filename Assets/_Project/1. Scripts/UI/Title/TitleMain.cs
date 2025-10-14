using System.Collections.Generic;
using RabbitDog;
using RabbitDog.UIExtensions;
using UnityEngine;
using RabbitDog.UIManagements;
using RabbitDog.Utility;
using UnityEngine.AddressableAssets;
using UnityEngine.Purchasing;

namespace Template
{
    [RegisterUILayer(UILayerType.Cover, "UILayerAddressConstants.TitleMain")]
    [RegisterScene("Title", "Scenes/Title.unity", typeof(TitleMain))]
    public class TitleMain : UILayer
    {
        [SerializeField] private AssetReferenceGameObject audioControllerRef;
        
        protected override void OnPreEnter(object param)
        {
            base.OnPreEnter(param);

            EnterAsync().Forget();
        }

        private async Awaitable EnterAsync()
        {
            await Awaitable.NextFrameAsync();
            await SceneTransition.FadeOutAsync();
     
            #region TitleTasks
            
            var audioControllerLoadingHandle = audioControllerRef.InstantiateAsync();
            TimerManager.Instance.Initialize(new TimerDataSourceImpl());
            
            // SpecDataManager.Instance.LoadFromResource();
            UserDataManager.Instance.Initialize();

            LocalNotificationManager.Initialize();
            LocalNotificationManager.RequestPermission();
            // 앱이벤트 UID 세팅
            // CAppEventLite.UID = (long)SystemInfo.deviceUniqueIdentifier.djb2Hash();

            // 파이어베이스 UID 세팅
            // LogManager.SetFirebaseSetUserID(SystemInfo.deviceUniqueIdentifier);

            // 앱이벤트
            // LogManager.progress(0);
            // LogManager.LOGIN();

            var handles = new List<Awaitable>();
            handles.Add(LocalizationManager.Instance.Initialize("Localization"));
            handles.Add(AlertMessageManager.Instance.Initialize("Prefabs/UI/Popup/Common/PopupToast.prefab"));
            
            //AppsFlyer 초기화
            // AppsflyerManager.Instance.AppsflyerInit();

            var items = new List<ProductCatalogItem>();
            var allShopSpecs = SpecDataManager.Instance.GetAllShopSpecs();
            for (var i = 0; i < allShopSpecs.Count; i++)
            {
                var specShop = allShopSpecs[i];
                if (string.IsNullOrEmpty(specShop.ProductId))
                {
                    continue;
                }

                items.Add(new ProductCatalogItem
                {
                    id = specShop.ProductId,
                    type = ProductType.Consumable,
                });
            }
            var iapTask = CookAppsIapWrapper.Instance.Initialize(items, null);

            handles.Add(AtlasManager.Instance.Initialize("Data/AtlasManager.asset"));
    #if __DEV && __SRD
            SRDebug.Init();
    #endif
            SceneTransition.Create<SceneTransition_Image>(SceneTransition_Image.TitleImagePath);
            handles.Add(SceneTransition.FadeInAsync());

            for (var i = 0; i < handles.Count; i++)
            {
                await handles[i];
            }
            await TopPanelSingleUseHelper.Instance.Initialize("Prefabs/UI/Common/TopPanelBar/TopPanelContainer.prefab");
            await iapTask;
            var audioController = await audioControllerLoadingHandle.WaitUntilDone();
            audioController.name = "AudioController";
            DontDestroyOnLoad(audioController);
            SettingOptions.Initialize();
            #endregion

            SceneUILayerManager.Instance.ChangeScene("Lobby");
        }

        protected override void OnBackButton(ref bool offPrevUI) { }
    }
}