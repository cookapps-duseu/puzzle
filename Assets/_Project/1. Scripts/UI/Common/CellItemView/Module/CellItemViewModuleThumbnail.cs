using CookApps;
using CookApps.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Template
{
    public class CellItemViewModuleThumbnail : CellItemViewModuleBase
    {
        [SerializeField] private SpriteLoader imgThumbnail;

        public override void Init(IReward data)
        {
            base.Init(data);

            imgThumbnail.gameObject.SetActive(true);
            // imgThumbnail.SetSprite(data.SpriteName).Forget();
        }
    }
}