
namespace Game.Data
{
    /// <summary>
    /// Class that manages all resource type class instances
    /// </summary>
    public class ResourceTypeManager : TypeClassLoader<ResourceType>
    {
        public ResourceTypeManager() : base("resources.json")
        {

        }

        private static ResourceTypeManager _instance;

        /// <summary>
        /// Global instance of the resource type manager
        /// </summary>
        public static ResourceTypeManager Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new ResourceTypeManager();

                return _instance;
            }
        }
    }
}