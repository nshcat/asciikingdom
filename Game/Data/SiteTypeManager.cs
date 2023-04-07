
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

                // Get the array
                var moduleArray = innerObj.GetArray("modules");

                foreach(var moduleNode in moduleArray)
                {
                    var moduleObj = new DeserializationHelper(moduleNode.AsObject());
                    var moduleTypeId = moduleObj.ReadValue<string>("type");

                    if (moduleObj.Node.ContainsKey("init"))
                    {
                        var initObj = moduleObj.GetObject("init");
                        typeClass.Modules[moduleTypeId] = initObj.Node;
                    }
                    else
                    {
                        typeClass.Modules[moduleTypeId] = new JsonObject();
                    }
                }

                types.Add(typeClass);
            }

            return types;
        }
    }
}