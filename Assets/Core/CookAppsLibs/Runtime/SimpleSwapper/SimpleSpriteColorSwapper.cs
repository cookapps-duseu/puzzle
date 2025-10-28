using UnityEngine;

namespace CookApps
{
    public class SimpleSpriteColorSwapper : SimpleSpriteBaseSwapper
    {
        [SerializeField] private SerializableDictionary<SimpleSwapType, Color> colors;
        [SerializeField] private SimpleSwapType currentType;
        public override SimpleSwapType CurrentType => currentType;

        private void Awake()
        {
            if (colors.TryGetValue(currentType, out var color))
                spriteRenderer.color = color;
        }

        public override void Swap(SimpleSwapType swapType)
        {
            if (currentType == swapType)
                return;

            if (!colors.ContainsKey(swapType))
                return;

            currentType = swapType;
            spriteRenderer.color = colors[swapType];
        }
    }
}
