using UnityEngine;

namespace CookApps.UIManagements
{
    public class UIManagementsConst : IUIManagementsConst
    {
        private float INCH_TO_CM = 2.54f;
        private float DRAG_THRESHORD_CM = 0.25f;

        public int DragThreshold
        {
            get
            {
                var dpi = Screen.dpi;
                if (dpi <= 0)
                {
                    dpi = 160f; // Fallback DPI
                }

                return Mathf.RoundToInt(DRAG_THRESHORD_CM / 2.54f * dpi);
            }
        }

        public static IUIManagementsConst Default { get; set; } = new UIManagementsConst();
    }

    public interface IUIManagementsConst
    {
        int DragThreshold { get; }
    }
}
