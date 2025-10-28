using TMPro;
using UnityEngine;

namespace CookApps
{
    public class SimpleTextMaterialSwapper : SimpleTextBaseSwapper
    {
        [SerializeField] private SerializableDictionary<SimpleSwapType, Material> materials;
        [SerializeField] private SimpleSwapType currentType;
        public override SimpleSwapType CurrentType => currentType;

        private void Awake()
        {
            if (materials.TryGetValue(currentType, out var material))
                text.fontMaterial = material;
        }

        public override void Swap(SimpleSwapType swapType)
        {
            if (currentType == swapType)
                return;

            if (!materials.ContainsKey(swapType))
                return;

            currentType = swapType;
            text.fontMaterial = materials[swapType];
        }
    }
}
