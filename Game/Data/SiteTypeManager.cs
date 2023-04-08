
using Game.Serialization;
using Game.Simulation.Sites;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Game.Data
{
    /// <summary>
    /// Class that manages all site type classes
    /// </summary>
    public class SiteTypeManager : TypeClassLoader<SiteType>
    {
        public SiteTypeManager()
            : base("sites.json")
        {

        }

        private static SiteTypeManager _instance;

        /// <summary>
        /// Global instance of the site type manager
        /// </summary>
        public static SiteTypeManager Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new SiteTypeManager();

                return _instance;
            }
        }

        /// <summary>
        /// Custom deserialization routine for site type classes
        /// </summary>
        /// <param name="jsonContents"></param>
        /// <returns></returns>
        protected override List<SiteType> DeserializeTypes(string jsonContents)
        {
            var types = new List<SiteType>();
            var root = JsonNode.Parse(jsonContents);
            var rootArray = root.AsArray();

            foreach(var innerNode in rootArray)
            {
                var innerObj = new DeserializationHelper(innerNode.AsObject());
                var typeClass = new SiteType();
                typeClass.Identifier = innerObj.ReadValue<string>("identifier");
                typeClass.TypeDescriptor = innerObj.ReadValue<string>("typedesc");

                // Get the modules object
                var moduleObject = innerObj.GetObject("modules");

                foreach(var kvp in moduleObject.Node)
                {
                    var moduleTypeId = kvp.Key;
                    var initNode = kvp.Value;
                    typeClass.Modules[moduleTypeId] = initNode.AsObject();
                }

                types.Add(typeClass);
            }

            return types;
        }
    }
}