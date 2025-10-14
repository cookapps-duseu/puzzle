using RabbitDog.UIManagements;

namespace Template
{
    [RegisterUILayer(UILayerType.Cover, "UILayerAddressConstants.LoadingMain")]
    [RegisterScene("SceneLoading", "Scenes/Loading.unity", typeof(LoadingMain))]
    public class LoadingMain : SceneLoading
    {
        
    }
}
