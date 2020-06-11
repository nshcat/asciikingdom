using Engine.Core;
using Engine.Graphics;

namespace Game.Simulation
{
    /// <summary>
    /// Represents a special site on the world map, such as cities, villages and colonies.
    /// </summary>
    public interface IWorldSite : IGameObject
    {
        /// <summary>
        /// The name for this site, specified by the user
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// A descriptive string for the type of this world site, such as "City" or "Village"
        /// </summary>
        public string TypeDescriptor { get; }
        
        /// <summary>
        /// Whether this sites name should be shown on the world map
        /// </summary>
        public bool ShowName { get; }
        
        /// <summary>
        /// The tile used to represent this site on the world map
        /// </summary>
        public Tile Tile { get; }
        
        /// <summary>
        /// The position of the site on the world map
        /// </summary>
        public Position Position { get; set; }
    }
}