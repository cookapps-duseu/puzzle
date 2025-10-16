using System.ComponentModel;
using UnityEngine;

#if __SRD
public partial class SROptions
{
    [Category("아이템")] public AssetType ItemType { get; set; } = AssetType.Coin;
    [Category("아이템")] public int ItemAmount { get; set; } = 1;

    [Category("아이템")]
    public void 하드커런시_지급()
    {
        UserDataManager.Instance.GetAssetData().AddAsset(ItemType, ItemAmount);
    }
}
#endif
