using UnityEngine;
using UnityEngine.UI;

namespace RabbitDog
{
    public class SimpleImageMaterialSwapper : SimpleImageBaseSwapper
    {
        [SerializeField] private SerializableDictionary<SimpleSwapType, Material> materials;
        [SerializeField] private SimpleSwapType currentType;
        public override SimpleSwapType CurrentType => currentType;

        private void Awake()
        {
            if (materials.TryGetValue(currentType, out var material))
                image.material = material;
        }

        public override void Swap(SimpleSwapType swapType)
        {
            if (currentType == swapType)
                return;

            currentType = swapType;
            materials.TryGetValue(currentType, out var material);
            image.material = material;
        }
    }
}
