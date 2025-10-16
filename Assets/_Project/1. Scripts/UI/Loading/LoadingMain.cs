using RabbitDog.UIManagements;

namespace Template
{
    [RegisterUILayer(UILayerType.Cover, UILayerAddressConstants.LoadingMain)]
    [RegisterScene("SceneLoading", UILayerAddressConstants.SceneLoading, typeof(LoadingMain))]
    public class LoadingMain : SceneLoading
    {
        
    }
}
