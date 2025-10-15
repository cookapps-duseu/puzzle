namespace Template
{
    public partial class SpecShop : ISpecShop
    {
        public int Id => id;

        public string ProductId => product_id;

        public float Price => price;

        private RewardBundle rewardBundle;
        
        public RewardBundle RewardGroups
        {
            get
            {
                CheckRewardBundle();
                return rewardBundle;
            }
        }
        private void CheckRewardBundle()
        {
            if (rewardBundle != null)
                return;

            rewardBundle = new RewardBundle();
            for (var i = 0; i < SpecDataManager.Instance.SpecRewardGroup.All.Count; i++)
            {
                var reward = SpecDataManager.Instance.SpecRewardGroup.All[i];
                if (reward.reward_group_id == reward_group_id)
                {
                    rewardBundle.Add(reward);
                }
            }
        }
    }
    
    public partial class SpecOption : ISpecOption
    {
        public string Value => value;
    }
    
    public partial class SpecConsumeGroup : IConsume
    {
        public AssetType ConsumeType
        {
            get
            {
                if (consume_type == RewardType.Asset)
                {
                    return (AssetType)consume_id;
                }
                return AssetType.None;
            }
        }

        public int ConsumeAmount => (int)consume_amount;
    }
    
    public partial class SpecRewardGroup : IReward
    {
        public RewardType RewardType => reward_type;
        public int RewardId => reward_id;
        public long RewardAmount => reward_amount;
    }
}
