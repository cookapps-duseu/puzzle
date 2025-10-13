using UnityEngine;

namespace RabbitDog
{
    [RequireComponent(typeof(ParticleSystem))]
    public class SimpleParticleColorSwapper : SimpleSwapper
    {
        [SerializeField] private ParticleSystem particle;
        [SerializeField] private SerializableDictionary<SimpleSwapType, Color> colors;
        [SerializeField] private SimpleSwapType currentType;
        public override SimpleSwapType CurrentType => currentType;

        private void Awake()
        {
            if (particle == null)
                particle = GetComponent<ParticleSystem>();

            var main = particle.main;

            if (colors.TryGetValue(currentType, out var color))
                main.startColor = color;
        }

        public override void Swap(SimpleSwapType swapType)
        {
            if (currentType == swapType)
                return;

            if (!colors.ContainsKey(swapType))
                return;

            currentType = swapType;
            var main = particle.main;
            main.startColor = colors[swapType];
        }
    }
}
