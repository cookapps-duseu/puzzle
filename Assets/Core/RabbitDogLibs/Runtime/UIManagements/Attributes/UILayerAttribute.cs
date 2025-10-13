using System;

namespace RabbitDog.UIManagements
{
    public class RegisterUILayerAttribute : Attribute
    {
#if UNITY_EDITOR
        public UILayerType LayerType { get; }
        public string AddressableName { get; }
#endif

        public RegisterUILayerAttribute(UILayerType layerType, string addressableName)
        {
#if UNITY_EDITOR
            LayerType = layerType;
            AddressableName = addressableName;
#endif
        }
    }

    public class RegisterSceneAttribute : Attribute
    {
#if UNITY_EDITOR
        public string SceneName { get; }
        public string SceneAddressableName { get; }
        public Type[] DefaultUILayers { get; }
#endif

        public RegisterSceneAttribute(string sceneName, string sceneAddressableName, params Type[] defaultUILayers)
        {
#if UNITY_EDITOR
            SceneName = sceneName;
            SceneAddressableName = sceneAddressableName;
            DefaultUILayers = defaultUILayers;
#endif
        }
    }
    
    public class GenerateUILayerDatabaseAttribute : Attribute
    {
        
    }
}
