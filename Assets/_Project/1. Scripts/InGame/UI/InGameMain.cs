using CookApps.UIManagements;
using CookApps.Utility;

namespace Template
{
    public class InGameMain : UILayer
    {
        public void Initialize()
        {
            
        }
        
        protected void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                OnClickPause();
            }
        }

        protected override void OnBackButton(ref bool offPrevUI)
        {
            OnClickPause();
        }
        
        public void OnClickPause()
        {
            SceneUILayerManager.Instance.PushUILayerAsync<PopupInGamePause>().Forget();
        }
    }
}
