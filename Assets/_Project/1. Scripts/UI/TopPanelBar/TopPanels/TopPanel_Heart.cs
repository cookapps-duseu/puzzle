using System;
using System.Collections.Generic;
using RabbitDog.Utility;
using TMPro;
using UnityEngine;

namespace Template
{
    public class TopPanel_Heart : TopPanelBase
    {
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private GameObject plusBtn;

        public override TopPanelType PanelType => TopPanelType.Heart;
        private List<(int type, int id)> timerListenerInfos = new();
        private HeartNode heartNode;
        

        private void Awake()
        {
            heartNode = new HeartNode();
            heartNode.SetComponents(currencyText, timerText);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            heartNode = null;
        }

        private void OnEnable()
        {
            UserAssetDataContainer.OnAssetDataChanged += HeartChanged;
            // timerListenerInfos.Add((TimerType.InfiniteHeartTimer, TimerManager.Instance.AddListener(TimerType.InfiniteHeartTimer, TimerEventType.TimerAdded, HeartTimerChanged)));
            // timerListenerInfos.Add((TimerType.InfiniteHeartTimer, TimerManager.Instance.AddListener(TimerType.InfiniteHeartTimer, TimerEventType.TimerEnd, HeartTimerChanged)));
            // timerListenerInfos.Add((TimerType.IncrementHeart, TimerManager.Instance.AddListener(TimerType.IncrementHeart, TimerEventType.TimerAdded, HeartTimerChanged)));
            // timerListenerInfos.Add((TimerType.IncrementHeart, TimerManager.Instance.AddListener(TimerType.IncrementHeart, TimerEventType.TimerEnd, HeartTimerChanged)));
            Refresh();
        }

        private void OnDisable()
        {
            UserAssetDataContainer.OnAssetDataChanged -= HeartChanged;
            foreach (var info in timerListenerInfos)
                TimerManager.Instance.RemoveListener(info.type, info.id);
            timerListenerInfos.Clear();
        }

        private void HeartTimerChanged(DateTime startTime, DateTime endTime)
        {
            Refresh();
        }

        private void HeartChanged(AssetType type)
        {
            // if (type != AssetType.Heart)
            //     return;
            // Refresh();
        }
        
        public void OnClick()
        {
            // if (!plusBtn.activeSelf)
            //     return;
            //
            // SceneUILayerManager.Instance.PushUILayerAsync<PopupHeartBuy>().Forget();
        }
        
        private float elapsedTime = 0f;
        private void Update()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime < 0.3333333f)
                return;
            elapsedTime = 0f;
            Refresh();
        }
        
        private void Refresh()
        {
            // var res = heartNode.Refresh();
            // plusBtn.SetActive(res == CurrentHeartState.Normal);
        }
    }
}
