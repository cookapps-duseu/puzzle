using System;
using System.Collections.Generic;
using CookApps.UIManagements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CookApps.UIExtensions
{
    public enum DefaultClickSoundType
    {
        None = -1,
        Basic,
        Custom_0,
        Custom_1,
        Custom_2,
    }

    [AddComponentMenu("UI/CAButton")]
    public class CAButton : Button
    {
        [SerializeField] private bool isBlockDrag = false;
        [SerializeField] private bool useDefaultClickSound = true;
        [SerializeField] private DefaultClickSoundType defaultClickSoundType;
        [SerializeField] private bool forceClickable = false;        
        [SerializeField] private SimpleSwapper[] swappers;

        public static event Action<DefaultClickSoundType> OnPlayDefaultClickSound;

        protected bool isClickable = true;

        public virtual void SetClickableState(bool enabled)
        {
            isClickable = enabled;
            swappers.Swap(enabled ? SimpleSwapType.Normal : SimpleSwapType.Disabled);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!forceClickable && !isClickable)
                return;
            
            if (!SelectableBlockerManager.Instance.IsAllowSelectable(name))
            {
                return;
            }

            SelectableBlockerManager.Instance.OnClicked(gameObject.name);
            if (useDefaultClickSound)
            {
                OnPlayDefaultClickSound?.Invoke(defaultClickSoundType);
            }

            base.OnPointerClick(eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (!forceClickable && !isClickable)
                return;

            if (!SelectableBlockerManager.Instance.IsAllowSelectable(name))
            {
                return;
            }

            SelectableBlockerManager.Instance.OnClicked(gameObject.name);
            if (useDefaultClickSound)
            {
                OnPlayDefaultClickSound?.Invoke(defaultClickSoundType);
            }

            base.OnSubmit(eventData);
        }
    }
}
