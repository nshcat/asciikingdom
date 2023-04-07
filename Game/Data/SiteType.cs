using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Game.Data
{
    /// <summary>
    /// Type class for site templates. Contains metadata, types of contained modules and their initializers.
    /// </summary>
    public class SiteType : ITypeClass
    {
        /// <summary>
        /// Unique identifier of this type
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// The human-readable description of this site type
        /// </summary>
        public string TypeDescriptor { get; set; }

        /// <summary>
        /// What types of modules this type of site contains, associated with the JSON object
        /// containing the initialization info for each module type
        /// </summary>
        public Dictionary<string, JsonObject> Modules { get; set; }
            = new Dictionary<string, JsonObject>();
    }
}
