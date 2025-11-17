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
            SceneUILayerManager.Instance.Initialize();
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
}
