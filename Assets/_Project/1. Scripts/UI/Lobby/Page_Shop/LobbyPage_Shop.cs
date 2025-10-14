using System.Collections.Generic;
using RabbitDog.UIExtensions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Template
{
    public class LobbyPage_Shop : LobbyPageBase
    {
        [SerializeField] private ShopView shopView;
        [SerializeField] private Transform topPanelAttachPoint;

        private List<AsyncOperationHandle> handles = new ();
        
        protected override void OnEnter()
        {
            base.OnEnter();
            // lobbyMain.TopPanelBar.AttachTo(CachedTr);
            TopPanelSingleUseHelper.Instance.GetPanel(TopPanelType.Heart).gameObject.SetActive(false);
            shopView.Init(true, false);
            //Appsflyer 앱이벤트
            // AppsflyerManager.Instance.SendStoreOpenEvent();
            
            // var specs = SpecDataManager.Instance.SpecShop.All;
            // for (var i = 0; i < specs.Count; i++)
            // {
            //     var spec = specs[i];
            //     handles.Add(Addressables.LoadAssetAsync<GameObject>($"Prefabs/UI/Pool/Shop/Shop_PackageSlot_{spec.id}.prefab"));
            // }
        }
        
        protected override void OnExit()
        {
            base.OnExit();
            for (var i = 0; i < handles.Count; i++)
            {
                var handle = handles[i];
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            handles.Clear();
        }
    }
}
