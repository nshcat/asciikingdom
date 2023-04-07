using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Game.Serialization;

namespace Game.Simulation.Sites
{
    /// <summary>
    /// Specialized deserialization helper with extra methods for sites
    /// </summary>
    public class SiteSerializationHelper
        : SerializationHelper
    {
        /// <summary>
        /// Reference to the current entity manager
        /// </summary>
        public SiteManager Manager { get; protected set; }

        public SiteSerializationHelper(SiteManager manager)
            : base()
        {
            this.Manager = manager;
        }

        public SiteSerializationHelper(SiteManager manager, JsonObject node)
            : base(node)
        {
            this.Manager = manager;
        }

        public override SerializationHelper WriteObject(string key)
        {
            var result = base.WriteObject(key);

            return new SiteSerializationHelper(this.Manager, result.Node);
        }
    }
}
