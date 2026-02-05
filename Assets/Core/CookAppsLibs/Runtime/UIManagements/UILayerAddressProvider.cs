namespace CookApps.UIManagements
{
    internal static class UILayerAddressProvider
    {
        private static IUILayerAddressProvider _provider;

        internal static void SetProvider(IUILayerAddressProvider provider)
        {
            _provider = provider;
        }

        public static string GetUILayerAddress(string uiLayerName)
        {
            return _provider?.GetUILayerAddress(uiLayerName) ?? string.Empty;
        }

        public static string GetSceneAddress(string sceneName)
        {
            return _provider?.GetSceneAddress(sceneName) ?? string.Empty;
        }
    }
}
