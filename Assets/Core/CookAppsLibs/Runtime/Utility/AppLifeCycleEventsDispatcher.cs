using System;
using UnityEngine;

namespace CookApps.Utility
{
    public class AppLifeCycleEventsDispatcher : SingletonMonoBehaviour<AppLifeCycleEventsDispatcher>
    {
        public static event Action OnQuit;
        public static event Action OnPause;
        public static event Action OnResume;
        public static event Action OnFocus;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            _ = Instance;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                OnPause?.Invoke();
            }
            else
            {
                OnResume?.Invoke();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                OnFocus?.Invoke();
            }
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            OnQuit?.Invoke();
        }
    }
}
