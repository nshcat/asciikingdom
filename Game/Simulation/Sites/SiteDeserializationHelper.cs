using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Game.Serialization;
using Game.Simulation;

namespace Game.Simulation.Sites
{
    /// <summary>
    /// Specialized deserialization helper with extra methods for sites
    /// </summary>
    public class SiteDeserializationHelper
        : DeserializationHelper
    {
        /// <summary>
        /// Reference to the current entity manager
        /// </summary>
        public SiteManager Manager { get; protected set; }

        public SiteDeserializationHelper(SiteManager manager, JsonObject node)
            : base(node)
        {
            this.Manager = manager;
        }

        /// <summary>
        /// Loads a world site reference in the form of a GUID and retrieves the corresponding world site
        /// </summary>
        /// <param name="key">JSON element key</param>
        /// <returns>World site referenced by the GUID read from the JSON node</returns>
        public WorldSite ResolveSiteReference(string key)
        {
            CheckSubKey(key);

            JsonNode node;
            var success = Node.TryGetPropertyValue(key, out node);
            if (!success)
                throw new ArgumentException($"Could not retrieve site reference for key \"{key}\"");

            var idString = node.GetValue<string>();

            Guid id;
            success = Guid.TryParse(idString, out id);
            if (!success)
                throw new JsonException($"Could not resolve site reference for key \"{key}\": \"{idString}\" is not a valid Guid");

            // Try to find site with the retrieved guid
            if (!Manager.HasSite(id))
                throw new JsonException($"Could not resolve site reference for key \"{key}\": No site with id \"{idString}\" exists");

            return Manager.GetSite(id);
        }

        public override DeserializationHelper GetObject(string key)
        {
            var result = base.GetObject(key);
            return new SiteDeserializationHelper(this.Manager, result.Node);
        }
    }
}
