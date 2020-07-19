using System.Collections.Generic;
using Engine.Core;
using Engine.Graphics;
using Game.Serialization;
using Game.Simulation.Modules;

namespace Game.Simulation
{
    /// <summary>
    /// A village is a small settlement associated with a city which produces basic, raw resources and is
    /// taxed by its associated city.
    /// </summary>
    public class Village : WorldSite
    {
        #region Properties
        /// <summary>
        /// Village names are not shown on the world map
        /// </summary>
        public override bool ShowName => false;

        /// <summary>
        /// The city this village is associated with
        /// </summary>
        public City AssociatedCity { get; set; }
        #endregion
        
        #region Modules
        /// <summary>
        /// The population controller module associated with this site
        /// </summary>
        protected PopulationController Population { get; set; }
        #endregion
        
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
            : base(name, position)
        {
            this.Name = name;
            this.Position = position;
            this.AssociatedCity = associatedCity;
    
            this.Population = new PopulationController(this, GrowthStages, initialPopulation);
            this.AddModule(this.Population);
            this.AddModule(new MapLabelRenderer(this, MapLabelStyle.Normal));
        }
        
        public override void Update(int weeks)
        {
            // TODO
        }
        
        /// <summary>
        /// Create a simulation view from this object
        /// </summary>
        public VillageView ToView()
        {
            return new VillageView
            {
                Id = this.Id,
                Name = this.Name,
                Population = this.Population,
                Position = this.Position
            };
        }
    }
}