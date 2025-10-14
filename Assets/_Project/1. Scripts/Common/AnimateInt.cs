using UnityEngine;

namespace Template
{
    public class AnimateInt
    {
        public int start = 0;
        public int curr = 0;
        public int target = 0;
        public float duration = 0.75f;

        public bool Update(float dt)
        {
            if (curr == target)
                return false;

            if (curr < target)
            {
                var increase = (int)((target - start) * (dt / duration));
                if (increase == 0)
                    increase = 1;
                curr += increase;
                if (curr > target)
                {
                    curr = target;
                }
                return true;
            }

            var decrease = Mathf.CeilToInt((start - target) * (dt / duration));
            if (decrease == 0)
                decrease = 1;
            curr -= decrease;
            if (curr < target)
            {
                curr = target;
            }
            return true;
        }

        public void SetTarget(int target)
        {
            start = curr;
            this.target = target;
        }

        public void ForceSet()
        {
            curr = start = target;
        }
    }
}
