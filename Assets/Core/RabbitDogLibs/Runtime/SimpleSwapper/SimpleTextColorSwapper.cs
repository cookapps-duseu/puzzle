using TMPro;
using UnityEngine;

namespace RabbitDog
{
    public class SimpleTextColorSwapper : SimpleTextBaseSwapper
    {
        [SerializeField] private SerializableDictionary<SimpleSwapType, Color> colors;
        [SerializeField] private SimpleSwapType currentType;
        public override SimpleSwapType CurrentType => currentType;

        private void Awake()
        {
            if (colors.TryGetValue(currentType, out var color))
                text.color = color;
        }

        public override void Swap(SimpleSwapType swapType)
        {
            if (currentType == swapType)
                return;

            if (!colors.ContainsKey(swapType))
                return;

            currentType = swapType;
            text.color = colors[swapType];
        }
    }
}
