using Engine.Core;
using SixLabors.ImageSharp.Formats.Gif;

namespace Game.Simulation
{
    /// <summary>
    /// Represents a collection of metadata that belongs to a world
    /// </summary>
    public class WorldMetadata
    {
        /// <summary>
        /// The dimensions of the map
        /// </summary>
        public Size Dimensions { get; set; }
        
        /// <summary>
        /// The seed used to generate this world
        /// </summary>
        public int Seed { get; set; }
        
        /// <summary>
        /// The relative scale of the overview map in relation to the detailed map
        /// </summary>
        public float OverviewScale { get; set; }

        /// <summary>
        /// User-specified name of the world
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Construct a new world metadata instance
        /// </summary>
        public WorldMetadata(Size dimensions, int seed, float overviewScale = 0.1250f)
        {
            this.Dimensions = dimensions;
            this.Seed = seed;
            this.OverviewScale = overviewScale;
        }
    }
}