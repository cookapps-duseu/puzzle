using CookApps.UIManagements;
using UnityEngine;
using UnityEngine.UI;

namespace CookApps.UIExtensions
{
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("UI/Simple Close Button")]
    public class SimpleCloseButton : MonoBehaviour
    {
        private CAButton button;
        
        private void Awake()
        {
            button = GetComponent<CAButton>();
            button.onClick.AddListener(OnClickClose);
        }
        
        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClickClose);
        }
        
        private void OnClickClose()
        {
            var ui = GetComponentInParent<UILayer>();
            SceneUILayerManager.Instance.PopUILayer(ui);
        }
    }
}
