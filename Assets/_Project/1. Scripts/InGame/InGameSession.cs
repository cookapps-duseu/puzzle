using Cysharp.Threading.Tasks;
using CookApps.UIManagements;
using CookApps.Utility;
using UnityEngine;

namespace Template
{
    public static class InGameSession
    {
        public static bool StartScene(InGameEnterInfo enterInfo)
        {
            if (enterInfo == null)
                return false;

            InGameSession.enterInfo = enterInfo;
            StartSceneInternal().Forget();
            return true;
        }

        private static InGameEnterInfo enterInfo;
        private static async UniTask StartSceneInternal()
        {
            SceneTransition.Create<SceneTransition_Image>(SceneTransition_Image.LoadingImagePath);
            await SceneTransition.FadeInAsync();
            SceneLoading.GoToNextScene("InGame", new SceneLoadingEventReceiver(OnInGameEnterLoading, OnNextSceneLoaded), enterInfo);
        }

        private static async UniTask OnInGameEnterLoading()
        {
            // 0.5초 고정대기
            await UniTask.WaitForSeconds(0.5f);
        }
        
        private static void OnNextSceneLoaded()
        {
            LoadInGameEntities().Forget();
        }
        
        private static async UniTask LoadInGameEntities()
        {
            await UniTask.NextFrame();
            var manager = Object.FindFirstObjectByType<InGameManager>();
            var inGameUI = SceneUILayerManager.Instance.GetUILayer<InGameMain>();
            inGameUI.Initialize();
            await SceneTransition.FadeOutAsync();

            ((IStateMachine)manager).AddNextState<FlowStateInGameStart>();

            SoundManager.StopAll();
            SoundManager.PlayInGameBGM();
        }

        public static async UniTask EndScene(LobbyEnterInfo lobbyEnterInfo)
        {
            GameObjectPoolManager.Clear();
            SceneTransition.Create<SceneTransition_Image>(SceneTransition_Image.LoadingImagePath);
            await SceneTransition.FadeInAsync();
            SceneLoading.GoToNextScene("Lobby", null, lobbyEnterInfo);
        }

        public static async UniTask RestartScene(InGameEnterInfo enterInfo)
        {
            GameObjectPoolManager.Clear();
            InGameSession.enterInfo = enterInfo;
            StartSceneInternal().Forget();
        }

        public enum MakeResult
        {
            Success,
            Fail
        }

        public static MakeResult MakeInGameEnterInfo(out InGameEnterInfo enterInfo)
        {
            enterInfo = null;
            return MakeResult.Fail;
        }

    }
}
