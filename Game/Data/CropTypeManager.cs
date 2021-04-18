
namespace Game.Data
{
    /// <summary>
    /// Class that manages all resource type class instances
    /// </summary>
    public class CropTypeManager : TypeClassLoader<CropType>
    {
        public CropTypeManager() : base("crops.json")
        {

        }

        private static CropTypeManager _instance;

        /// <summary>
        /// Global instance of the crop type manager
        /// </summary>
        public static CropTypeManager Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new CropTypeManager();

                return _instance;
            }
        }
    }
}