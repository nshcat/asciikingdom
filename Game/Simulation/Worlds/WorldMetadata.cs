using Engine.Core;
using SixLabors.ImageSharp.Formats.Gif;

namespace Game.Simulation.Worlds
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
        /// The initial "spawn" location that is on the initially discovered continent
        /// </summary>
        public Position InitialLocation { get; set; }

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
            Dimensions = dimensions;
            Seed = seed;
            OverviewScale = overviewScale;
        }

        /// <summary>
        /// Parameterless constructor for deserialization
        /// </summary>
        public WorldMetadata()
        {

        }
    }
}