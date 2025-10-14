namespace Template
{
    public interface ISpecOption
    {
        public string Value { get; }
    }
    
    public interface ISpecShop
    {
        public int Id { get; }
        public string ProductId { get; }
        public int Price { get; }
        
        public RewardBundle RewardGroups { get; }
    }
    
    public interface IReward
    {
        public RewardType RewardType { get; }
        public int RewardId { get; }
        public long RewardAmount { get; }
    }
    
    public interface ISpecConsumeGroup
    {
        public AssetType CostAssetType { get; }
        public int CostAmount { get; }
    }
}
