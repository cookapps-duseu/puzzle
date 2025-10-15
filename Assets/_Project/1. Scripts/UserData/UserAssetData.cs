using System;
using System.Collections.Generic;
using MemoryPack;
using RabbitDog.Utility;
using UnityEngine;

namespace Template
{
    [MemoryPackable]
    public partial class UserAsset
    {
        public AssetType AssetType { get; set; }
        public int Amount { get; set; }
        public int Extra { get; set; }

        private const int ExtraLowMask = 0x0000FFFF; // Extra의 하위 16비트

        public short ExtraLow
        {
            get => unchecked((short)(Extra & ExtraLowMask));
            set
            {
                var lowBits = (int)(ushort)value;
                Extra = (Extra & ~ExtraLowMask) | lowBits;
            }
        }

        public short ExtraHigh
        {
            get => unchecked((short)((Extra >> 16) & ExtraLowMask));
            set
            {
                var highBits = ((int)(ushort)value) << 16;
                Extra = (Extra & ExtraLowMask) | highBits;
            }
        }

        public void SetExtraShorts(short low, short high)
        {
            var highBits = ((int)(ushort)high) << 16;
            var lowBits = (int)(ushort)low;
            Extra = highBits | lowBits;
        }

        public (short low, short high) GetExtraShorts()
        {
            return (ExtraLow, ExtraHigh);
        }
    }

    [MemoryPackable]
    public partial class UserAssetData
    {
        public List<UserAsset> assets = new();
    }
    
    public sealed class UserAssetDataContainer : UserDataContainerBase<UserAssetData>
    {
        public override string PreferenceKey => "UserAssetData";

        public override void InitData()
        {
            base.InitData();

            var startCoin = 0;//SpecOptionDict.StartCoin;
            CreateNewAsset(AssetType.Coin, startCoin);
            GetAsset(AssetType.Coin).Extra = 0;
            CreateNewAsset(AssetType.Heart, 5);

        }
        
        public static event Action<AssetType> OnAssetDataChanged;

        private UserAsset CreateNewAsset(AssetType assetType, int amount = 0, int extra = 0)
        {
            for (var i = 0; i < data.assets.Count; i++)
            {
                if (data.assets[i].AssetType == assetType)
                    return data.assets[i];
            }

            var asset = new UserAsset
            {
                AssetType = assetType,
                Amount = amount,
                Extra = extra
            };
            
            data.assets.Add(asset);
            return asset;
        }

        private UserAsset GetAsset(AssetType assetType)
        {
            for (var i = 0; i < data.assets.Count; i++)
            {
                if (data.assets[i].AssetType == assetType)
                    return data.assets[i];
            }
    
            // 찾지 못한 경우 새 에셋 생성
            return CreateNewAsset(assetType);
        }
        
        public int GetAssetAmount(AssetType assetType)
        {
            return GetAsset(assetType).Amount;
        }
        
        public int GetAssetExtra(AssetType assetType)
        {
            return GetAsset(assetType).Extra;
        }

        public (short low, short high) GetAssetExtraShorts(AssetType assetType)
        {
            return GetAsset(assetType).GetExtraShorts();
        }

        public short GetAssetExtraLow(AssetType assetType)
        {
            return GetAsset(assetType).ExtraLow;
        }

        public short GetAssetExtraHigh(AssetType assetType)
        {
            return GetAsset(assetType).ExtraHigh;
        }

        public void SetExtra(AssetType assetType, int extra)
        {
            var asset = GetAsset(assetType);
            asset.Extra = extra;
            isDirty = true;
            OnAssetDataChanged?.Invoke(assetType);
        }

        public void SetExtraShorts(AssetType assetType, short low, short high)
        {
            var asset = GetAsset(assetType);
            asset.SetExtraShorts(low, high);
            isDirty = true;
            OnAssetDataChanged?.Invoke(assetType);
        }

        public void SetExtraLow(AssetType assetType, short value)
        {
            var asset = GetAsset(assetType);
            asset.ExtraLow = value;
            isDirty = true;
            OnAssetDataChanged?.Invoke(assetType);
        }

        public void SetExtraHigh(AssetType assetType, short value)
        {
            var asset = GetAsset(assetType);
            asset.ExtraHigh = value;
            isDirty = true;
            OnAssetDataChanged?.Invoke(assetType);
        }

        public void DecreaseExtra(AssetType assetType, int amount)
        {
            var asset = GetAsset(assetType);
            asset.Extra -= amount;
            isDirty = true;
            OnAssetDataChanged?.Invoke(assetType);
        }

        public bool CanUseAsset(AssetType assetType, int amount)
        {
            var asset = GetAsset(assetType);
            return asset.Amount >= amount;
        }

        public bool UseAsset(AssetType assetType, int amount)
        {
            if (amount <= 0)
                return false;
            
            var asset = GetAsset(assetType);
            if (asset.Amount < amount)
                return false;
            
            if (asset.AssetType == AssetType.Heart && asset.Amount == GetMaxHeart())
            {
                // 하트 사용시 타이머 시작
                asset.Extra = TimeSystem.GetUtcTime().ToIntTimestamp();
            }

            asset.Amount -= amount;
            OnAssetDataChanged?.Invoke(assetType);
            isDirty = true;
            return true;
        }
        
        public bool ConsumeAssets(List<IConsume> consumeGroups)
        {
            // 선검사
            for (var i = 0; i < consumeGroups.Count; i++)
            {
                var group = consumeGroups[i];
                if (!CanUseAsset(group.ConsumeType, group.ConsumeAmount))
                    return false;
            }
            
            // 실제 소모
            for (var i = 0; i < consumeGroups.Count; i++)
            {
                var group = consumeGroups[i];
                UseAsset(group.ConsumeType, group.ConsumeAmount);
            }

            return true;
        }
        
        public void AddReward(IReward reward)
        {
            if (reward.RewardType == RewardType.Asset)
            {
                var assetType = (AssetType)reward.RewardId;
                AddAsset(assetType, (int)reward.RewardAmount);
            }
        }
        
        public void AddAsset(AssetType assetType, int amount)
        {
            if (amount <= 0)
                return;
            
            if (assetType.IsBuffType())
            {
                var timerId = assetType.GetBuffTimerType();
                if (timerId != 0)
                {
                    TimerManager.Instance.AddTimer(timerId, amount);
                }
            }
            else
            {
                var asset = GetAsset(assetType);
                asset.Amount += amount;
                if (asset.AssetType == AssetType.Heart)
                {
                    asset.Amount = Mathf.Min(asset.Amount, GetMaxHeart());
                }
                else if (asset.AssetType == AssetType.Coin)
                {
                    asset.Extra += amount; // 누적 코인 수익
                }
            }
            OnAssetDataChanged?.Invoke(assetType);
            isDirty = true;
        }
        
        public void CheckHeartTime()
        {
            var heartAsset = GetAsset(AssetType.Heart);
            var maxHeart = GetMaxHeart();
            if (heartAsset.Amount >= maxHeart)
            {
                return;
            }

            if (GetRemainHeartRecoverSeconds() > 0)
            {
                return;
            }

            int nowTimestamp = TimeSystem.GetUtcTime().ToIntTimestamp();
            int diff = nowTimestamp - GetHeartUseTimestamp();
            int lifeRecover = diff / SpecOptionDict.HeartRecoverSecond;
            int rechargeableLifeCount = maxHeart - heartAsset.Amount;
            lifeRecover = Mathf.Min(rechargeableLifeCount, lifeRecover);
            if (heartAsset.Amount < maxHeart)
            {
                AddAsset(AssetType.Heart, lifeRecover);
            }
            
            if (heartAsset.Amount < maxHeart)
            {
                heartAsset.Extra = nowTimestamp - diff % SpecOptionDict.HeartRecoverSecond;
                isDirty = true;
            }
        }

        public int GetRemainHeartRecoverSeconds()
        {
            int nowTimestamp = TimeSystem.GetUtcTime().ToIntTimestamp();
            int diff = nowTimestamp - GetHeartUseTimestamp();
            return Mathf.Max(0, SpecOptionDict.HeartRecoverSecond - diff);
        }

        public int GetRemainHeartFullRecoverSeconds()
        {
            int nowTimestamp = TimeSystem.GetUtcTime().ToIntTimestamp();
            int remainTime = SpecOptionDict.HeartRecoverSecond - (nowTimestamp - GetHeartUseTimestamp());
            int needRecoverCount = GetMaxHeart() - GetAssetAmount(AssetType.Heart);
            remainTime += (needRecoverCount - 1) * SpecOptionDict.HeartRecoverSecond;
            return Mathf.Max(0, remainTime);
        }

        public int GetHeartUseTimestamp()
        {
            var heartAsset = GetAsset(AssetType.Heart);
            return heartAsset.Extra;
        }
        
        public int GetMaxHeart()
        {
            var maxHeart = SpecOptionDict.MaxHeart;
            // if (TimerManager.Instance.IsRunning(TimerType.IncrementHeart))
            // {
            //     maxHeart += SpecOptionDict.AdditionalMaxHeart;
            // }
            
            return maxHeart;
        }
    }
}
