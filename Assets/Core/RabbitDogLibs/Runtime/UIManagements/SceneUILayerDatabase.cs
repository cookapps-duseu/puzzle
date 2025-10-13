using System;
using System.Collections.Generic;
using UnityEngine;

namespace RabbitDog.UIManagements
{
    public abstract class SceneUILayerDatabase
    {
        protected Dictionary<string, SceneData> sceneDataDict;

        public Dictionary<string, SceneData> SceneDataDict => sceneDataDict;
        
        protected Dictionary<Type, UILayerData> uiLayerDataDict;
        public Dictionary<Type, UILayerData> UILayerDataDict => uiLayerDataDict;

        public abstract Sprite GetDimLayerSprite();
        
        protected SceneUILayerDatabase()
        {
            SceneUILayerManager.OnUITransitionEvent += OnUITransitionEvent;
        }

        ~SceneUILayerDatabase()
        {
            SceneUILayerManager.OnUITransitionEvent -= OnUITransitionEvent;
        }
        
        private void OnUITransitionEvent(UILayerTransition transition, string key, UILayer uiLayer)
        {
            if (transition == UILayerTransition.EnterFinished)
            {
                for (var i = 0; i < uiLayer.PreloadAddressables.Length; i++)
                {
                    uiLayer.PreloadAddressables[i].LoadAssetAsync();
                }
            }
            
            if (transition == UILayerTransition.ExitFinished)
            {
                for (var i = 0; i < uiLayer.PreloadAddressables.Length; i++)
                {
                    uiLayer.PreloadAddressables[i].ReleaseAsset();
                }
            }
        }
    }
}
