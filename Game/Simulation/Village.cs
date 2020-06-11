using System.Collections.Generic;
using Engine.Core;
using Engine.Graphics;

namespace Game.Simulation
{
    /// <summary>
    /// A village is a small settlement associated with a city which produces basic, raw resources and is
    /// taxed by its associated city.
    /// </summary>
    public class Village : PopulatedSite
    {
        /// <summary>
        /// The name of this village
        /// </summary>
        public override string Name { get; set; }
        
        /// <summary>
        /// The position of this village on the world map
        /// </summary>
        public override Position Position { get; set; }
        
        /// <summary>
        /// The city this village is associated with
        /// </summary>
        public City AssociatedCity { get; set; }
        
        /// <summary>
        /// List of village growth stages
        /// </summary>
        private static List<SiteGrowthStage> GrowthStages { get; } = new List<SiteGrowthStage>
        {
            new SiteGrowthStage(0, "Small Hamlet", new Tile(61, Color.FromHex("#CDD2D2"))),
            new SiteGrowthStage(25, "Hamlet", new Tile(240, Color.FromHex("#CDD2D2"))),
            new SiteGrowthStage(50, "Small Village", new Tile(145, Color.FromHex("#CDD2D2"))),
            new SiteGrowthStage(100, "Village", new Tile(146, Color.FromHex("#CDD2D2"))),
        };
        
        /// <summary>
        /// Create a bew village with given name, position, initial population and associated city
        /// </summary>
        public Village(string name, Position position, int initialPopulation, City associatedCity)
            : base(GrowthStages, initialPopulation)
        {
            this.Name = name;
            this.Position = position;
            this.AssociatedCity = associatedCity;
        }
        
        public override void Update(int weeks)
        {
            throw new System.NotImplementedException();
        }
    }
}