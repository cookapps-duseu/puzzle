using System.Collections.Generic;
using CookApps.Iap.Result;
using RabbitDog.UIExtensions;
using RabbitDog.UIManagements;
using RabbitDog.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace Template
{
    public class ShopView : MonoBehaviour
    {
        [SerializeField] private TableView tableView;
        [SerializeField] private GameObject goFoldBtn;

        private IReadOnlyList<ISpecShop> specs;

        private bool isFolded = false;
        private bool isShowSequence = false;

        private void OnEnable()
        {
            CookAppsIapWrapper.OnPurchaseSuccess += OnAfterPurchase;
        }

        private void OnDisable()
        {
            CookAppsIapWrapper.OnPurchaseSuccess -= OnAfterPurchase;
        }

        private ObjectPool<GameObject> tableViewPool;

        private GameObject OnGetTableViewCellItem(int idx)
        {
            var go = tableViewPool.Get();
            var slot = go.GetComponent<ShopProductNode>();
            slot.UpdateUI(specs[idx]);
            return slot.CachedGo;
        }

        private void OnReleaseTableViewCellItem(int idx, GameObject obj)
        {
            tableViewPool.Release(obj);
        }

        private Vector2 OnGetTableViewCellItemSize(int idx)
        {
            return new Vector2(1000, 534);
        }

        private int OnGetTotalTableViewCellItemCount()
        {
            return specs.Count;
        }

        private void Awake()
        {
            tableView.OnGetTotalCellItemCount += OnGetTotalTableViewCellItemCount;
            tableView.OnGetCellItemSize += OnGetTableViewCellItemSize;
            tableView.OnReleaseCellItem += OnReleaseTableViewCellItem;
            tableView.OnGetCellItem += OnGetTableViewCellItem;

            tableViewPool = new ObjectPool<GameObject>(
                () =>
                {
                    var go = new GameObject("ShopProductNode", typeof(ShopProductNode), typeof(RectTransform));
                    var rectTr = go.GetComponent<RectTransform>();
                    rectTr.sizeDelta = new Vector2(1000, 200);
                    go.transform.SetParent(tableView.content, false);
                    return go;
                },
                obj => obj.SetActive(true),
                obj => obj.SetActive(false),
                Destroy,
                false
            );
        }

        protected void OnDestroy()
        {
            tableViewPool.Dispose();
        }

        internal void Init(bool isShowSequence, bool initialFolded)
        {
            isFolded = initialFolded;
            this.isShowSequence = isShowSequence;

            InitView();
        }

        public void OnClickFoldBtn()
        {
            isFolded = !isFolded;
            InitView();
        }

        private void InitView()
        {
            InitData();
            // if (goFoldBtn != null)
            // {
            //     goFoldBtn.SetActive(_isFolded);
            //     goFoldBtn.transform.SetAsLastSibling();
            // }
            tableView.RefreshAll();
        }

        private void InitData()
        {
            specs = SpecDataManager.Instance.GetAllShopSpecs();
        }

        private void OnAfterPurchase(PurchaseResult result)
        {
            var shopInfo = SpecDataManager.Instance.GetSpecShopByProductId(result.ProductId);
            InitView();
            PurchaseProcess(shopInfo).Forget();
        }
        
        public async Awaitable PurchaseProcess(ISpecShop shopInfo)
        {
            // 연출
            // var popup = await SceneUILayerManager.Instance.PushUILayerAsync<PopupPurchaseResult>();
            // if (isShowSequence)
            // {
            //     if (shopInfo.group is not ShopGroupType.CoinPack)
            //     {
            //         var lobbyMain = SceneUILayerManager.Instance.GetUILayer<LobbyMain>();
            //         lobbyMain.GoTo(LobbyPageType.Lobby);
            //     }
            // }
            // await popup.WaitForExit();
            // var data = new PopupItemRewardEffect.PopupItemRewardEffectParam();
            // for (var i = 0; i < shopInfo.RewardGroups.Count; i++)
            // {
            //     data.AddReward(new RewardData(shopInfo.RewardGroups[i], shopInfo.RewardGroups[i].RewardAmount));
            // }
            // await SceneUILayerManager.Instance.PushUILayerAsync<PopupItemRewardEffect>(data);
        }
    }
}
