using Engine.Graphics;

namespace Game.Simulation
{
    /// <summary>
    /// A state of growth a sites population can be in
    /// </summary>
    public class SiteGrowthStage
    {
        /// <summary>
        /// The population threshold after which the site is considered to be in this
        /// particular growth stage
        /// </summary>
        public int PopulationThreshold { get; set; }
        
        /// <summary>
        /// The tile to use to display a site in this growth stage on the world map
        /// </summary>
        public Tile Tile { get; set; }
        
        /// <summary>
        /// A textual descriptor of this growth stage, such as "City" or "Town".
        /// </summary>
        public string Descriptor { get; set; }

        /// <summary>
        /// Construct a new growth stage instance
        /// </summary>
        public SiteGrowthStage(int populationThreshold, string descriptor, Tile tile)
        {
            PopulationThreshold = populationThreshold;
            Tile = tile;
            Descriptor = descriptor;
        }
    }
}