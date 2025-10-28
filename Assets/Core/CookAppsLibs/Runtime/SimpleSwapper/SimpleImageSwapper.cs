using UnityEngine;
using UnityEngine.UI;

namespace CookApps
{
    public class SimpleImageSwapper : SimpleImageBaseSwapper
    {
        [SerializeField] private SerializableDictionary<SimpleSwapType, Sprite> sprites;
        [SerializeField] private SimpleSwapType currentType;
        public override SimpleSwapType CurrentType => currentType;

        private void Awake()
        {
            if (sprites.TryGetValue(currentType, out var sprite))
                image.sprite = sprite;
        }

        public override void Swap(SimpleSwapType swapType)
        {
            if (currentType == swapType)
                return;

            if (!sprites.ContainsKey(swapType))
                return;

            currentType = swapType;
            image.sprite = sprites[swapType];
        }
    }
}
