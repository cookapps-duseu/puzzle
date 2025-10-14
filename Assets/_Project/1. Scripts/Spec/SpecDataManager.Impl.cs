using System.Collections.Generic;
using RabbitDog;

namespace Template
{
    public partial class SpecDataManager : SingletonMonoBehaviour<SpecDataManager>
    {
        public ISpecOption GetOption(string key)
        {
            throw new System.NotImplementedException();
        }
        
        public IReadOnlyList<ISpecShop> GetAllShopSpecs()
        {
            throw new System.NotImplementedException();
        }

        public ISpecShop GetSpecShopByProductId(string productId)
        {
            throw new System.NotImplementedException();
        }
    }
}
