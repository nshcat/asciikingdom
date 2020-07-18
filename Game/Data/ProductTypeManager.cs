
namespace Game.Data
{
    /// <summary>
    /// Class that manages all product type class instances
    /// </summary>
    public class ProductTypeManager : TypeClassLoader<ProductType>
    {
        public ProductTypeManager() : base("products.json")
        {

        }

        private static ProductTypeManager _instance;

        /// <summary>
        /// Global instance of the product type manager
        /// </summary>
        public static ProductTypeManager Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new ProductTypeManager();

                return _instance;
            }
        }
    }
}