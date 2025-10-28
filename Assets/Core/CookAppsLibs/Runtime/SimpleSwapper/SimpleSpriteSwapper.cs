using UnityEngine;

namespace CookApps
{
    public class SimpleSpriteSwapper : SimpleSpriteBaseSwapper
    {
        [SerializeField] private SerializableDictionary<SimpleSwapType, Sprite> sprites;
        [SerializeField] private SimpleSwapType currentType;

        public override SimpleSwapType CurrentType => currentType;

        private void Awake()
        {
            if (sprites.TryGetValue(currentType, out var sprite))
                spriteRenderer.sprite = sprite;
        }

        public override void Swap(SimpleSwapType swapType)
        {
            if (currentType == swapType)
                return;

            if (!sprites.ContainsKey(swapType))
                return;

            currentType = swapType;
            spriteRenderer.sprite = sprites[swapType];
        }
    }
}
