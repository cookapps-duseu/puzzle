using TMPro;
using UnityEngine;

namespace Template
{
    public class CellItemViewModuleAmount : CellItemViewModuleBase
    {
        [Header("Required")]
        [SerializeField] private TMP_Text txtAmount;

        [Header("Optional")]
        [SerializeField] private GameObject goTime;
        [SerializeField] private TMP_Text txtTime;

        public override void Init(IReward data)
        {
            base.Init(data);
        }
    }
}
