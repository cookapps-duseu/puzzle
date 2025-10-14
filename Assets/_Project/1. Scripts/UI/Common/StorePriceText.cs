using Cysharp.Text;
using TMPro;
using UnityEngine;

namespace Template
{
    public class StorePriceText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txtPrice;
        private ISpecShop specShop;

        public void SetSpecData(ISpecShop specShop)
        {
            this.specShop = specShop;
            UpdateText();
        }

        public void Awake()
        {
            CookAppsIapWrapper.OnStoreInitialized += OnStoreInitialized;
        }

        public void OnDestroy()
        {
            CookAppsIapWrapper.OnStoreInitialized -= OnStoreInitialized;
        }

        private void OnStoreInitialized()
        {
            UpdateText();
        }

        private void OnEnable()
        {
            UpdateText();
        }

        // Update is called once per frame
        private void UpdateText()
        {
            if (specShop == null)
            {
                return;
            }

            string productId = specShop.ProductId;
            string price = CookAppsIapWrapper.Instance.GetPriceString(productId);

            if (!string.IsNullOrEmpty(price))
            {
                txtPrice.text = price;
                return;
            }

            price = ZString.Format("$ {0}", specShop.Price);
            txtPrice.text = price;
        }
    }
}
