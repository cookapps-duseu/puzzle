using UnityEngine;
using UnityEngine.UI;

namespace CookApps
{
    public class SimpleRectSizeSwapper : SimpleRectBaseSwapper
    {
        [SerializeField] private SerializableDictionary<SimpleSwapType, Vector2> sizes;
        [SerializeField] private SimpleSwapType currentType;
        public override SimpleSwapType CurrentType => currentType;

        private void Awake()
        {
            if (sizes.TryGetValue(currentType, out var size))
                SetSize(size);
        }

        public override void Swap(SimpleSwapType swapType)
        {
            if (currentType == swapType)
                return;

            if (!sizes.ContainsKey(swapType))
                return;

            currentType = swapType;
            SetSize(sizes[swapType]);
        }
        
        private void SetSize(Vector2 size)
        {
            rectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }
    }
}
