using UnityEngine;
using UnityEngine.UI;

namespace RabbitDog
{
    public class SimpleImageColorSwapper : SimpleImageBaseSwapper
    {
        [SerializeField] private SerializableDictionary<SimpleSwapType, Color> colors;
        [SerializeField] private SimpleSwapType currentType;
        public override SimpleSwapType CurrentType => currentType;

        private void Awake()
        {
            if (colors.TryGetValue(currentType, out var color))
                image.color = color;
        }

        public override void Swap(SimpleSwapType swapType)
        {
            if (currentType == swapType)
                return;

            if (!colors.ContainsKey(swapType))
                return;

            currentType = swapType;
            image.color = colors[swapType];
        }
    }
}
