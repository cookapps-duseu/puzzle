using System;
using Cysharp.Threading.Tasks;
using CookApps.UIExtensions;
using CookApps.UIManagements;
using CookApps.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace Template
{
    public class LobbyEnterInfo
    {
        public enum EnterState
        {
            None,
            Fail,
            Win,
        }

        public EnterState enterState;
        public int starCount;
    }
    
    public class LobbyMain : UILayer
    {
        [SerializeField] private LobbyPageController pageController;

        private LobbyEnterInfo lobbyEnterInfo;
        public LobbyEnterInfo LobbyEnterInfo => lobbyEnterInfo;
        
        LobbyEnterFlowRunner flowRunner;
        
        public TopPanelBar TopPanelBar { get; private set; }

        public void ClearLobbyEnterData()
        {
            lobbyEnterInfo = null;
        }

        public T GetPage<T>() where T : LobbyPageBase
        {
            return pageController.GetPage<T>();
        }

        protected override void OnPreEnter(object param)
        {
            base.OnPreEnter(param);
            EnterAsync(param).Forget();
        }
        
        private async UniTask EnterAsync(object param)
        {
            //로비 BGM
            SoundManager.StopAll();
            SoundManager.PlayLobbyBGM();

            lobbyEnterInfo = param as LobbyEnterInfo;
            TopPanelBar = await TopPanelBar.AddToUILayer(this, TopPanelType.Coin, TopPanelType.Heart);

            pageController.Initialize();

            await SceneTransition.FadeOutAsync();

            RunEnterFlow();
        }

        protected override void OnPreExit()
        {
            base.OnPreExit();
            TopPanelBar.AttachTo(TopPanelBar.CachedRectTr);
            flowRunner?.Clear();
        }

        protected override void OnBackButton(ref bool offPrevUI)
        {
        }

        private void RunEnterFlow()
        {
            flowRunner = new LobbyEnterFlowRunner();
            flowRunner.StartRunFlow(() =>
            {
                ClearLobbyEnterData();
                flowRunner = null;
            });
        }
    }
}
