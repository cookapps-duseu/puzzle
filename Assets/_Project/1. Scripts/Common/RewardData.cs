using System.Collections.Generic;

namespace Template
{
    /// <summary>
    /// RewardData의 묶음 관리 클래스
    /// </summary>
    public class RewardBundle
    {
        public class Reward : IReward
        {
            public Reward(IReward reward)
            {
                RewardType = reward.RewardType;
                RewardId = reward.RewardId;
                RewardAmount = reward.RewardAmount;
            }
            
            public bool AddReward(IReward reward)
            {
                if (RewardType == reward.RewardType && RewardId == reward.RewardId)
                {
                    RewardAmount += reward.RewardAmount;
                    return true;
                }

                return false;
            }

            public RewardType RewardType { get; private set; }

            public int RewardId { get; private set; }

            public long RewardAmount { get; private set; }
        }
        
        private List<Reward> rewards = new();
        public IReadOnlyList<IReward> Rewards => rewards;

        public void Add(IReward reward)
        {
            for (var i = 0; i < rewards.Count; i++)
            {
                if (rewards[i].AddReward(reward))
                {
                    return;
                }
            }

            rewards.Add(new Reward(reward));
        }

        public void Merge(RewardBundle bundle)
        {
            foreach (var data in bundle.rewards)
            {
                Add(data);
            }
        }
    }
}
