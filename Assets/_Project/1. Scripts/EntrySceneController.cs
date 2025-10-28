using System.Collections;
using CookApps.UIExtensions;
using CookApps.UIManagements;
using CookApps.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Template
{
    public class EntrySceneController : MonoBehaviour
    {
        #region field - serialized
        [SerializeField] private AssetReferenceT<TMP_FontAsset> fallbackFont;
        #endregion

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        async void Start()
        {
            await Awaitable.NextFrameAsync();

            TMP_Settings.fallbackFontAssets.Clear();
            Application.targetFrameRate = 60;
            var h = fallbackFont.LoadAssetAsync<TMP_FontAsset>();
            await h.WaitUntilDone();
            if (h.Result != null)
                TMP_Settings.fallbackFontAssets.Add(h.Result);
            
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.black);
            tex.Apply();
            var dimLayerSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
            SceneUILayerManager.Instance.Initialize(new SceneUILayerDatabaseImpl(dimLayerSprite));
            SceneTransition.Create<SceneTransition_FadeInOut>();
            await SceneTransition.FadeInAsync();
            SceneUILayerManager.Instance.ChangeScene("Title");
            CAButton.OnPlayDefaultClickSound += (type) =>
            {
                if (type == DefaultClickSoundType.Basic)
                {
                    SoundManager.PlayClick();
                    HapticPatternsHelper.PlaySoftHaptic();
                }
            };
        }
    }

    [GenerateUILayerDatabase]
    public partial class SceneUILayerDatabaseImpl : SceneUILayerDatabase
    {
        private Sprite dimLayerSprite;
        public SceneUILayerDatabaseImpl(Sprite dimLayerSprite) 
        {
            this.dimLayerSprite = dimLayerSprite;
        }
        public override Sprite GetDimLayerSprite()
        {
            return dimLayerSprite;
        }
    }
}
