namespace Template
{
        public static class SpecEnumExtensions
    {
        public static bool IsBuffType(this AssetType assetType)
        {
            return assetType switch
            {
                _ => false,
            };
        }
        
        public static int GetBuffTimerType(this AssetType assetType)
        {
            return assetType switch
            {
                _ => 0,
            };
        }

    }
}