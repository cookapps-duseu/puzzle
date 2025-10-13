using UnityEngine;
using UnityEngine.Tilemaps;

namespace RabbitDog
{
    [RequireComponent(typeof(Tilemap))]
    public class SimpleTilemapColorSwapper : SimpleSwapper
    {
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private SerializableDictionary<SimpleSwapType, Color> colors;
        [SerializeField] private SimpleSwapType currentType;
        public override SimpleSwapType CurrentType => currentType;

        private void Awake()
        {
            if (tilemap == null)
                tilemap = GetComponent<Tilemap>();

            if (colors.TryGetValue(currentType, out var color))
                tilemap.color = color;
        }

        public override void Swap(SimpleSwapType swapType)
        {
            if (currentType == swapType)
                return;

            if (!colors.ContainsKey(swapType))
                return;

            currentType = swapType;
            tilemap.color = colors[swapType];
        }
    }
}
