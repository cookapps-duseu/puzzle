using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RabbitDog
{
    public enum SimpleSwapType
    {
        Normal = 0,
        Disabled = 1,

        Custom_0 = 200,
        Custom_1 = 201,
        Custom_2 = 202,
        Custom_3 = 203,
        Custom_4 = 204,
        Custom_5 = 205,
        Custom_6 = 206,
        Custom_7 = 207,
        Custom_8 = 208,
        Custom_9 = 209,
        Custom_10 = 210,
    }

    public abstract class SimpleSwapper : CachedMonoBehaviour
    {
        public abstract SimpleSwapType CurrentType { get; }
        public abstract void Swap(SimpleSwapType swapType);
    }

    [RequireComponent(typeof(Image))]
    public abstract class SimpleImageBaseSwapper : SimpleSwapper
    {
        [SerializeField] protected Image image;
    }
    
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class SimpleSpriteBaseSwapper : SimpleSwapper
    {
        [SerializeField] protected SpriteRenderer spriteRenderer;
    }
    
    [RequireComponent(typeof(TMP_Text))]
    public abstract class SimpleTextBaseSwapper : SimpleSwapper
    {
        [SerializeField] protected TMP_Text text;
    }

    public static class SimpleSwapperExtensions
    {
        public static void Swap(this IReadOnlyList<SimpleSwapper> swappers, SimpleSwapType swapType)
        {
            for (var i = 0; i < swappers.Count; i++)
            {
                swappers[i].Swap(swapType);
            }
        }
    }
}
