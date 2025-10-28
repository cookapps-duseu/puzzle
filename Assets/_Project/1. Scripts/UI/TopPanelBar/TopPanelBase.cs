using CookApps;
using TMPro;
using UnityEngine;

namespace Template
{
    public enum TopPanelType
    {
        Coin = 0,
        Heart,
    }

    public abstract class TopPanelBase : CachedMonoBehaviour
    {
        [SerializeField] protected TMP_Text currencyText;
        [SerializeField] private Transform icon;
        
        public Transform Icon => icon;
        public abstract TopPanelType PanelType { get; }
    }
}
