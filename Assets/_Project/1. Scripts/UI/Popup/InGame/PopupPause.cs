using RabbitDog.UIManagements;

namespace Template
{
    [RegisterUILayer(UILayerType.Popup, "UILayerAddressConstants.PopupInGamePause")]
    public class PopupInGamePause : UILayer
    {
        protected override void OnPreEnter(object param)
        {
            base.OnPreEnter(param);
        }
    }
}
