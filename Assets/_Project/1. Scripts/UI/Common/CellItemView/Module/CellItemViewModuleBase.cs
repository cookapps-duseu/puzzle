using UnityEngine;

namespace Template
{
    public class CellItemViewModuleBase : MonoBehaviour
    {
        private IReward data;

        public virtual void Init(IReward data)
        {
            this.data = data;
        }
    } 
}
