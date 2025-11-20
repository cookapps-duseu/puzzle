using System;

namespace CookApps.Utility
{
    public class AppLifeCycleEventsDispatcher : SingletonMonoBehaviour<AppLifeCycleEventsDispatcher>
    {
        public static event Action OnQuit;
        public static event Action OnPause;
        public static event Action OnResume;
        public static event Action OnFocus;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
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
