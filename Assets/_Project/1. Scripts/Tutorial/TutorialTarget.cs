using System;
using RabbitDog;
using UnityEngine;

namespace Template
{
    public class TutorialTarget : CachedMonoBehaviour
    {
        [SerializeField] private string targetName;
 
        public string TargetName => targetName;
        
        private void Awake()
        {
            TutorialManager.Instance.RegisterTarget(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            TutorialManager.Instance.UnregisterTarget(this);
        }
    }
}
