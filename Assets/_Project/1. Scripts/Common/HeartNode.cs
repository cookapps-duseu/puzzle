using System;
using TMPro;
using UnityEngine;
using CookApps;

namespace Template
{
    public enum CurrentHeartState
    {
        Normal,
        Infinite,
        Full
    }
    
    [Serializable]
    public class HeartNode
    {
        [SerializeField] private TMP_Text heartCount;
        [SerializeField] private TMP_Text heartTimer;
        [SerializeField] private SimpleSwapper[] infiniteSwappers;

        public void SetComponents(TMP_Text heartCount, TMP_Text heartTimer, SimpleSwapper[] infiniteSwappers = null)
        {
            this.heartCount = heartCount;
            this.heartTimer = heartTimer;
            this.infiniteSwappers = infiniteSwappers;
        }
        
        public CurrentHeartState Refresh()
        {
            // first, check if infinite heart is active
            // var infiniteEndTime = TimerManager.Instance.GetEndTime(TimerType.InfiniteHeartTimer);
            // infiniteSwappers?.Swap(infiniteEndTime > ServerTime.GetServerTime() ? SimpleSwapType.Normal : SimpleSwapType.Disabled);
            // if (infiniteEndTime > ServerTime.GetServerTime())
            // {
            //     var remaining = infiniteEndTime - ServerTime.GetServerTime();
            //     // infinite heart is active
            //     heartCount.text = "∞";
            //     heartTimer.text = remaining.ToLeftTimeStringHHMMSS();
            //     return CurrentHeartState.Infinite;
            // }
            //
            // var assetData = UserDataManager.Instance.GetAssetData();
            // var heart = assetData.GetAssetAmount(AssetType.Heart);
            // var startTimestamp = assetData.GetAssetExtra(AssetType.Heart);            
            // heartCount.SetText(heart);
            // if (heart >= assetData.GetMaxHeart())
            // {
            //     // heart is full
            //     heartTimer.text = "LobbyHome_Heart_Full".Localize();
            //     return CurrentHeartState.Full;
            // }
            //
            // {
            //     int endTimestamp = startTimestamp + SpecOptionDict.HeartRecoverTime;
            //     var remaining = endTimestamp - ServerTime.GetServerTime().ToIntTimestamp();
            //     heartTimer.SetText(remaining.ToLeftTimeStringHHMMSS());
            //     return CurrentHeartState.Normal;
            // }
            return CurrentHeartState.Normal;
        }
    }
}
