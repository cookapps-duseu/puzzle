using System;
using CookApps.UIManagements;
using UnityEngine;
using TMPro;

namespace Template
{
    [RegisterUILayer(UILayerType.Modal, UILayerAddressConstants.PopupCommon)]
    public class PopupCommon : UILayer
    {
        private class PopupCommonParam
        {
            public string title;
            public string desc;
            public string btnYText;
            public string btnNText;
            public Action escapeAction;
        }

        public static async Awaitable<PopupCommon> Show(string title, string desc, string buttonYText, string buttonNText, Action<object> closeCallback)
        {
            var param = new PopupCommonParam
            {
                title = title,
                desc = desc,
                btnYText = buttonYText,
                btnNText = buttonNText
            };
            
            return await SceneUILayerManager.Instance.PushUILayerAsync<PopupCommon>(param, closeCallback);
        }
        
        [SerializeField] private TMP_Text txtTitle;
        [SerializeField] private TMP_Text txtDesc;
        [SerializeField] private TMP_Text txtButtonY;
        [SerializeField] private TMP_Text txtButtonN;

        private PopupCommonParam param = null;
        private int _timeSec = 0;

        protected override void OnPreEnter(object param)
        {
            base.OnPreEnter(param);
            var popupParam = param as PopupCommonParam;
            
            txtTitle.text = popupParam.title;
            txtDesc.text = popupParam.desc;
            txtButtonY.text = popupParam.btnYText;
            txtButtonN.text = popupParam.btnNText;
        }

        protected override void OnPostExit()
        {
            base.OnPostExit();
            StopAllCoroutines();
        }
        
        public void OnClickButtonY()
        {
            SceneUILayerManager.Instance.PopUILayer(this, 1);
        }

        public void OnClickButtonN()
        {
            SceneUILayerManager.Instance.PopUILayer(this, 2);
        }

        protected override void OnBackButton(ref bool offPrevUI)
        {
            OnClickButtonN();
        }
    }
}
