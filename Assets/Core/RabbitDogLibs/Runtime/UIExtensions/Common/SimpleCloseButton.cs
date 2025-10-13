using RabbitDog.UIManagements;
using UnityEngine;
using UnityEngine.UI;

namespace RabbitDog.UIExtensions
{
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("UI/RabbitDog Simple Close Button")]
    public class SimpleCloseButton : MonoBehaviour
    {
        private RDButton button;
        
        private void Awake()
        {
            button = GetComponent<RDButton>();
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
