using MemoryPack;

namespace Template
{
    [MemoryPackable]
    public sealed partial class UserBasicData
    {
        
    }

    public class UserBasicDataContainer : UserDataContainerBase<UserBasicData>
    {
        public override string PreferenceKey => "UserBasicData";
        
        public override void InitData()
        {
            base.InitData();
        }
    }
}
