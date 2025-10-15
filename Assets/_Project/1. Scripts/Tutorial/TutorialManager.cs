using System;
using System.Collections.Generic;
using CookApps.Inspector;
using RabbitDog;
using UnityEngine;

namespace Template
{
    public class TutorialManager : SingletonMonoBehaviour<TutorialManager>
    {
        [SerializeField, ReadOnly] private List<TutorialTarget> targets = new ();

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
            targets.Clear();
            onEndTutorial = null;
        }
        
        public void RegisterTarget(TutorialTarget target)
        {
            if (!targets.Contains(target))
            {
                targets.Add(target);
            }
        }

        public void UnregisterTarget(TutorialTarget target)
        {
            targets.Remove(target);
        }

        private bool TryFindTarget(string targetName, out TutorialTarget target)
        {
            target = null;
            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i].TargetName == targetName)
                {
                    target = targets[i];
                    return true;
                }
            }

            return false;
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
