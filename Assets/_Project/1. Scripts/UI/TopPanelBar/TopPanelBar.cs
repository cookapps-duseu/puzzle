using CookApps.UIManagements;
using UnityEngine;

namespace Template
{
    [RegisterUILayer(UILayerType.Overlay, UILayerAddressConstants.TopPanelBar)]
    public class TopPanelBar : UILayer
    {
        private static int inc;

        public static async Awaitable<TopPanelBar> AddToUILayer(UILayer targetUI, params TopPanelType[] ownPanelTypes)
        {
            return await SceneUILayerManager.Instance.PushUILayerAsync<TopPanelBar>($"TopPanelBar_{inc++}", (targetUI, ownPanelTypes));
        }

        [SerializeField] private RectTransform panelParent;

        private TopPanelType[] usePanelTypes;
        public TopPanelType[] UsePanelTypes => usePanelTypes;

        private UILayer targetUI;
        public UILayer TargetUI => targetUI;

        protected override void OnPreEnter(object param)
        {
            base.OnPreEnter(param);
            (targetUI, usePanelTypes) = ((UILayer, TopPanelType[])) param;
            TopPanelSingleUseHelper.Instance.Push(this);
            SceneUILayerManager.OnUITransitionEvent += OnUITransitionEvent;
        }

        protected override void OnPreExit()
        {
            base.OnPreExit();
            TopPanelSingleUseHelper.Instance.Pop(this);
            SceneUILayerManager.OnUITransitionEvent -= OnUITransitionEvent;
        }

        private void OnUITransitionEvent(UILayerTransition transaction, string uiKey, UILayer ui)
        {
            if (transaction == UILayerTransition.Exiting && ui == targetUI)
            {
                CloseThisUILayer();
            }
        }

        protected override void OnBackButton(ref bool offPrevUI)
        {
            offPrevUI = true;
        }

        public void AddPanel(TopPanelType type, RectTransform panel)
        {
            panel.SetParent(panelParent, false);
            TopPanelSingleUseHelper.Instance.ApplyLayout(type, panel);
        }
        
        public void AttachTo(Transform parent)
        {
            panelParent.SetParent(parent, false);
        }
    }
}