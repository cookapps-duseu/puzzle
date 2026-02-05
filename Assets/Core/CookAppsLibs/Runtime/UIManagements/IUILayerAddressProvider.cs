namespace CookApps.UIManagements
{
    internal interface IUILayerAddressProvider
    {
        string GetUILayerAddress(string uiLayerName);
        string GetSceneAddress(string sceneName);
    }
}
