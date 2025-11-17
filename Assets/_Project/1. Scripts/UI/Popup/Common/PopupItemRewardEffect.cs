using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;
using CookApps.UIManagements;
using CookApps.Utility;
using TMPro;
using Random = UnityEngine.Random;

namespace Template
{
    public class PopupItemRewardEffect : UILayer
    {
        public class PopupItemRewardEffectParam
        {
            public List<(IReward reward, bool hasSrcPos, Vector3 srcPos)> dataList = new ();
            
            public void AddReward(IReward reward, Vector3 srcPosition)
            {
                dataList.Add((reward, true, srcPosition));
            }
            
            public void AddReward(IReward reward)
            {
                dataList.Add((reward, false, default));
            }
        }
        
        [SerializeField] private UIBezierLauncher bezierLauncher;
        [SerializeField] private UIBezierLauncher coinLauncher;
        [SerializeField] private UIBezierMover coinMoverOrigin;
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private RectTransform srcPositionRoot;
        [SerializeField] private float radius;
        [SerializeField] private float radiusHole;
        [SerializeField] private CellItemViewCommon itemViewOrigin;

        private List<CellItemViewCommon> itemViews = new ();
        private bool isRunning = false;
        public PopupItemRewardEffectParam effectData;
        private bool isCoinChecked = false;

        protected override void Awake()
        {
            base.Awake();
            coinLauncher.OnLaunched += OnLaunched;
            coinLauncher.OnArrived += OnCoinArrived;
            coinLauncher.OnArrived += OnArrived;
            coinLauncher.OnDestroyed += OnCoinLauncherDestroyed;
            bezierLauncher.OnLaunched += OnLaunched;
            bezierLauncher.OnArrived += OnArrived;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            coinLauncher.OnLaunched -= OnLaunched;
            coinLauncher.OnArrived -= OnCoinArrived;
            coinLauncher.OnArrived -= OnArrived;
            coinLauncher.OnDestroyed -= OnCoinLauncherDestroyed;
            bezierLauncher.OnLaunched -= OnLaunched;
            bezierLauncher.OnArrived -= OnArrived;
        }

        protected override void OnPreEnter(object param)
        {
            base.OnPreEnter(param);
            var originPos = srcPositionRoot.position;
            effectData = param as PopupItemRewardEffectParam;

            var hasPlayButton = ObjectRegistry.Instance.TryGetObject(RegistryKey.PlayButton, out var playButton);
            var hasCoinPanel = ObjectRegistry.Instance.TryGetObject(RegistryKey.CoinPanel, out var coinPanel);
            var hasHeartPanel = ObjectRegistry.Instance.TryGetObject(RegistryKey.CoinPanel, out var heartPanel);
            
            for (var i = 0; i < effectData.dataList.Count; i++)
            {
                var data = effectData.dataList[i];
                var reward = data.reward;
                if (reward.RewardType != RewardType.Asset)
                    continue;
                var assetType = (AssetType)reward.RewardId;
                if (assetType == AssetType.Coin && hasCoinPanel)
                {
                    var moverCount = Mathf.Clamp(Mathf.Log(reward.RewardAmount, 2), 1, 15);
                    for (var j = 0; j < moverCount; j++)
                    {
                        var moverGo = Instantiate(coinMoverOrigin.CachedGo, CachedRectTr);
                        moverGo.SetActive(true);
                        var coinMover = moverGo.GetComponent<UIBezierMover>();
                        if (!data.hasSrcPos)
                        {
                            data.srcPos = coinMoverOrigin.CachedTr.position;
                        }
                        coinMover.startPoint = data.srcPos;
                        coinMover.endPoint = coinPanel.CachedTr.position;
                        coinLauncher.AddMover(coinMover);
                    }
                    coinText.SetTextFormat("+{0}", reward.RewardAmount);
                    coinText.transform.position = data.srcPos;
                    coinText.transform.SetAsLastSibling();
                    coinText.gameObject.SetActive(true);
                    coinLauncher.Initialize(coinPanel.CachedTr);
                    coinLauncher.LaunchAsync().Forget();
                    continue;
                }

                var itemViewGo = Instantiate(itemViewOrigin.gameObject, CachedRectTr);
                var itemView = itemViewGo.GetComponent<CellItemViewCommon>();
                itemViewGo.SetActive(true);
                itemView.SetData(reward, null);
                itemViews.Add(itemView);
                var mover = itemView.GetComponent<UIBezierMover>();
                if (data.hasSrcPos)
                {
                    mover.startPoint = data.srcPos;
                }
                else
                {
                    srcPositionRoot.position = originPos;
                    var vector = Random.insideUnitCircle * radius;
                    srcPositionRoot.anchoredPosition += vector;
                    srcPositionRoot.anchoredPosition += vector.normalized * radiusHole;
                    mover.startPoint = srcPositionRoot.position;
                }

                mover.CachedTr.position = mover.startPoint;
                
                if (assetType is AssetType.Heart or AssetType.InfiniteHeart)
                {
                    mover.endPoint = hasHeartPanel ? heartPanel.CachedTr.position : mover.startPoint;
                }
                else if (assetType is AssetType.Coin)
                {
                    mover.endPoint = mover.startPoint;
                }
                else
                {
                    mover.endPoint = hasPlayButton ? playButton.CachedTr.position : mover.startPoint;
                }
                
                bezierLauncher.AddMover(mover);
            }

            bezierLauncher.Initialize();
            bezierLauncher.LaunchAsync().Forget();
            isRunning = true;
        }
        
        private void OnLaunched(int index, int totalCount)
        {
            SoundManager.PlaySFX("sfx_stage_clear_gold_start");
        }
        
        private void OnArrived(int index, int totalCount)
        {
            SoundManager.PlaySFX("sfx_stage_clear_gold_end");
        }
        
        private void OnCoinArrived(int index, int totalCount)
        {
            if (isCoinChecked)
                return;
            isCoinChecked = true;
            var coinAmount = 0;
            foreach (var (reward, _, _) in effectData.dataList)
            {
                if (reward.RewardType == RewardType.Asset && (AssetType)reward.RewardId == AssetType.Coin)
                {
                    coinAmount += (int)reward.RewardAmount;
                }
            }
            UserDataManager.Instance.GetAssetData().DecreaseExtra(AssetType.Coin, coinAmount);
        }
        
        private void OnCoinLauncherDestroyed()
        {
            if (isCoinChecked)
                return;

            var coinAmount = 0;
            foreach (var (reward, _, _) in effectData.dataList)
            {
                if (reward.RewardType == RewardType.Asset && (AssetType)reward.RewardId == AssetType.Coin)
                {
                    coinAmount += (int)reward.RewardAmount;
                }
            }
            UserDataManager.Instance.GetAssetData().DecreaseExtra(AssetType.Coin, coinAmount);
        }
    }
}
