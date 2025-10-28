using System;
using CookApps.UIExtensions;
using UnityEngine;

namespace Template
{
    public class CellItemViewCommon : MonoBehaviour
    {
        [SerializeField] private bool canClick = true;
        [SerializeField] private CellItemViewModuleBase[] modules;
        [SerializeField] private CAButton button;

        public IReward RewardData { get; private set; }

        private Action _action;

        private void OnDestroy()
        {
            RewardData = null;
            _action = null;
        }

        public void SetData(IReward rewardData, Action action)
        {
            RewardData = rewardData;
            if (action != null)
            {
                _action += action;
            }

            UpdateButton();
            UpdateView();
        }

        protected virtual void UpdateView()
        {
            foreach (var module in modules)
            {
                module.Init(RewardData);
            }
        }

        private void OnClick()
        {
            _action?.Invoke();
        }

        private void UpdateButton()
        {
            if (button != null)
            {
                button.enabled = canClick;
            }
        }
    }    
}
