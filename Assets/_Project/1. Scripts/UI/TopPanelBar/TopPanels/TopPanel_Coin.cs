using RabbitDog.UIManagements;
using RabbitDog.Utility;
using UnityEngine;

namespace Template
{
    public class TopPanel_Coin : TopPanelBase
    {
        public override TopPanelType PanelType => TopPanelType.Coin;

        private AnimateInt animateInt = new AnimateInt();
        
        private void OnEnable()
        {
            UserAssetDataContainer.OnAssetDataChanged += CoinChanged;
            CoinChanged(AssetType.Coin);
        }

        private void OnDisable()
        {
            UserAssetDataContainer.OnAssetDataChanged -= CoinChanged;
        }

        private void CoinChanged(AssetType type)
        {
            if (type != AssetType.Coin)
                return;
            
            var assetData = UserDataManager.Instance.GetAssetData();
            var coin = assetData.GetAssetAmount(type) - assetData.GetAssetExtra(type);
            animateInt.SetTarget(coin);
        }
        
        public void OnClick()
        {
            var lobbyMain = SceneUILayerManager.Instance.GetUILayer<LobbyMain>();
            if (lobbyMain == null)
            {
                // SceneUILayerManager.Instance.PushUILayerAsync<PopupShop>().Forget();
            }
            else
            {
                // lobbyMain.GoTo(LobbyPageType.Shop);
            }
        }

        private void Update()
        {
            if (!animateInt.Update(Time.deltaTime))
                return;

            Refresh();
        }
        
        private void Refresh()
        {
            // currencyText.SetTextFormat("{0:N0}", animateInt.curr);
        }
    }
}
