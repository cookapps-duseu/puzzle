using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CookApps.UIExtensions
{
    [ExecuteAlways]
    public class CASlider : UIBehaviour
    {
        /// <summary>
        /// Setting that indicates one of four directions.
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// From the left to the right
            /// </summary>
            LeftToRight,

            /// <summary>
            /// From the right to the left
            /// </summary>
            RightToLeft,

            /// <summary>
            /// From the bottom to the top.
            /// </summary>
            BottomToTop,

            /// <summary>
            /// From the top to the bottom.
            /// </summary>
            TopToBottom,
        }

        [SerializeField] private Direction direction = Direction.LeftToRight;
        [Range(0, 1)] [SerializeField] private float value;
        [SerializeField] private float minValue = 0;
        [SerializeField] private float maxValue = 1;
        [SerializeField] private bool wholeNumbers;
        [SerializeField] private RectTransform fillRect;
        [SerializeField] private Image fillImage;
        [SerializeField] private RectTransform handleRect;
        [SerializeField] private RectTransform handleContainerRect;
        [SerializeField] private TMP_Text text;
        [SerializeField] private string format = "{0:N0}/{1:N0}";

        // field is never assigned warning
#pragma warning disable 649
        private DrivenRectTransformTracker m_Tracker;
#pragma warning restore 649

        private bool isFirst = true;
        private float visualizedValue;

        public float Value
        {
            get => value;
            set
            {
                this.value = value;
                Set();
            }
        }

        public float normalizedValue
        {
            get
            {
                if (Mathf.Approximately(minValue, maxValue))
                    return 0;

                return Mathf.InverseLerp(minValue, maxValue, value);
            }
            set => this.value = Mathf.Lerp(minValue, maxValue, value);
        }

        private enum Axis
        {
            Horizontal = 0,
            Vertical = 1,
        }

        private Axis axis => direction == Direction.LeftToRight || direction == Direction.RightToLeft ? Axis.Horizontal : Axis.Vertical;
        private bool reverseValue => direction == Direction.RightToLeft || direction == Direction.TopToBottom;

        private float ClampValue(float input)
        {
            float newValue = Mathf.Clamp(input, minValue, maxValue);
            if (wholeNumbers)
            {
                newValue = Mathf.Round(newValue);
            }

            return newValue;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_Tracker.Clear();
        }

#if UNITY_EDITOR
        protected void Update()
        {
            Set();
        }
#endif

        protected virtual void Set()
        {
            float newValue = ClampValue(value);
            if (!isFirst && Mathf.Approximately(visualizedValue, newValue))
                return;

            isFirst = true;
            visualizedValue = newValue;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            m_Tracker.Clear();

            {
                m_Tracker.Add(this, fillRect, DrivenTransformProperties.Anchors);
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;

                if (fillImage != null && fillImage.type == Image.Type.Filled)
                {
                    fillImage.fillAmount = normalizedValue;
                }
                else
                {
                    if (reverseValue)
                    {
                        anchorMin[(int) axis] = 1 - normalizedValue;
                    }
                    else
                    {
                        anchorMax[(int) axis] = normalizedValue;
                    }
                }

                fillRect.anchorMin = anchorMin;
                fillRect.anchorMax = anchorMax;
            }

            if (handleContainerRect != null)
            {
                m_Tracker.Add(this, handleRect, DrivenTransformProperties.Anchors);
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;
                anchorMin[(int) axis] = anchorMax[(int) axis] = reverseValue ? 1 - normalizedValue : normalizedValue;
                handleRect.anchorMin = anchorMin;
                handleRect.anchorMax = anchorMax;
            }
        }
        
        public void SetValue(float cur, float max, string customText = null)
        {
            Value = cur / max * maxValue;
            if (text != null)
            {
                if (!string.IsNullOrEmpty(customText))
                {
                    text.text = customText;
                }
                else
                {
                    text.SetText(format, cur, max);
                }
            }
        }
    }
}