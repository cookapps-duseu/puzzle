using System;
using System.Collections.Generic;
using CookApps.Inspector;
using CookApps;
using UnityEngine;

namespace Template
{
    public class TutorialManager : SingletonMonoBehaviour<TutorialManager>
    {
        private TutorialBase currentTutorial;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            currentTutorial?.Kill();
            currentTutorial = null;
        }

        public bool RunTutorial<T>() where T : TutorialBase, new ()
        {
            if (currentTutorial != null)
                return false;

            currentTutorial = new T();
            currentTutorial.RunTutorial(OnEndTutorial);
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
