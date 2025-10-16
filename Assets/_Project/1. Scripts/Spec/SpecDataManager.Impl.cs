using System.Collections.Generic;
using RabbitDog;

namespace Template
{
    public partial class SpecDataManager : SingletonMonoBehaviour<SpecDataManager>
    {
        public ISpecOption GetOption(string key)
        {
            return SpecOption.Get(key);
        }
        
        public IReadOnlyList<ISpecShop> GetAllShopSpecs()
        {
            return SpecShop.All;
        }

        public ISpecShop GetSpecShopByProductId(string productId)
        {
            for (var i = 0; i < SpecShop.All.Count; i++)
            {
                if (SpecShop.All[i].product_id == productId)
                {
                    return SpecShop.All[i];
                }
            }

            return null;
        }
    }
}
