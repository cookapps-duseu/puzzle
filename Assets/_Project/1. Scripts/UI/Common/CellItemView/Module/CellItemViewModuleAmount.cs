using CookApps.Utility;
using Cysharp.Text;
using TMPro;
using UnityEngine;

namespace Template
{
    public class CellItemViewModuleAmount : CellItemViewModuleBase
    {
        [Header("Required")]
        [SerializeField] private TMP_Text txtAmount;

        [Header("Optional")]
        [SerializeField] private GameObject goTime;
        [SerializeField] private TMP_Text txtTime;

        public override void Init(IReward data)
        {
            base.Init(data);
            if (data.RewardType == RewardType.Asset && data.RewardId is (int)AssetType.InfiniteHeart)
            {
                var amount = (int)data.RewardAmount;
                if (goTime != null)
                {
                    txtAmount.gameObject.SetActive(false);
                    goTime.SetActive(true);

                    txtTime.text = amount.ToItemTime();
                }
                else
                {
                    txtAmount.gameObject.SetActive(true);
                    txtAmount.text = amount.ToItemTime();
                }

                return;
            }

            txtAmount.gameObject.SetActive(true);
            if (goTime != null) goTime.SetActive(false);

            txtAmount.gameObject.SetActive(true);
            if (data.RewardType == RewardType.Asset && data.RewardId == (int)AssetType.Coin)
                txtAmount.SetText(data.RewardAmount);
            else if (data.RewardAmount > 1)
                txtAmount.SetTextFormat("x{0}", data.RewardAmount);
            else
                txtAmount.gameObject.SetActive(false);
        }
    }
}
