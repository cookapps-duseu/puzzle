using CookApps;
using CookApps.UIManagements;
using CookApps.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Template
{
    public class SceneTransition_Image : SceneTransitionBase
    {
        public static string TitleImagePath => "Textures/Standalone/Img_Splash.png";
        public static string LoadingImagePath => "Textures/Standalone/Img_Transition_001.png";
        
        private float fadeInDuration = 0.1f;
        private float fadeOutDuration = 0.25f;

        private Image image;
        private AsyncOperationHandle<Sprite> spriteLoadHandle;
        
        public override void Initialize(object viewOption)
        {
            var touchBlocker = CachedGo.AddComponent<NonDrawingGraphic>();
            touchBlocker.raycastTarget = true;
            
            var imageGo = new GameObject("TransitionImage", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
            imageGo.transform.SetParent(CachedGo.transform, false);
            image = imageGo.GetComponent<Image>();
            image.enabled = false;
            var color = image.color;
            color.a = 0;
            image.color = color;
            spriteLoadHandle = Addressables.LoadAssetAsync<Sprite>(viewOption as string);
        }

        public override async Awaitable FadeInAsync()
        {
            await spriteLoadHandle.WaitUntilDone();
            var sprite = spriteLoadHandle.Result;
            image.sprite = sprite;
            var fitter = image.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
            image.enabled = true;
            Color color = image.color;
            float diff = 1f - color.a;
            while (color.a < 1f)
            {
                color.a += diff * Time.deltaTime / fadeInDuration;
                image.color = color;
                await Awaitable.NextFrameAsync();
            }
        }

        public override async Awaitable FadeOutAsync()
        {
            Color color = image.color;
            float diff = 0f - color.a;
            while (color.a > 0f)
            {
                color.a += diff * Time.deltaTime / fadeOutDuration;
                image.color = color;
                await Awaitable.NextFrameAsync();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            spriteLoadHandle.Release();
        }
    }
}