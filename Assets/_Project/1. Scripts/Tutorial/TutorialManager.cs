using System;
using System.Collections.Generic;
using CookApps.Inspector;
using RabbitDog;
using UnityEngine;

namespace Template
{
    public class TutorialManager : SingletonMonoBehaviour<TutorialManager>
    {
        private TutorialBase currentTutorial;
        private Action onEndTutorial;

        private void Awake()
        {
            onEndTutorial = OnEndTutorial;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            currentTutorial?.Kill();
            currentTutorial = null;
            onEndTutorial = null;
        }

        public bool RunTutorial<T>() where T : TutorialBase, new ()
        {
            if (currentTutorial != null)
                return false;

            currentTutorial = new T();
            currentTutorial.RunTutorial(onEndTutorial);
            return true;
        }
        
        private void OnEndTutorial()
        {
            currentTutorial = null;
        }
        
        public void KillCurrentTutorial()
        {
            currentTutorial?.Kill();
            currentTutorial = null;
        }
    }
}
