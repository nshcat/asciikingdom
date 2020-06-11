using System.Collections.Generic;
using Engine.Graphics;

namespace Game.Data
{
    /// <summary>
    /// Represents a type of special resource found on the map
    /// </summary>
    public class ResourceType : ITypeClass
    {
        /// <summary>
        /// Unique identifier of this type
        /// </summary>
        public string Identifier { get; set; }
        
        /// <summary>
        /// Resource name that is displayed to the player
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// A short, descriptive text about the resource
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// How this resource is drawn on the world map
        /// </summary>
        public Tile Tile { get; set; }
        
        /// <summary>
        /// Probabilistic weight of this resource. The higher, the more common it is.
        /// </summary>
        public double Weight { get; set; }
        
        /// <summary>
        /// Set of terrain types this resource is able to spawn in
        /// </summary>
        public HashSet<TerrainType> AllowedTerrain { get; set; }
    }
}