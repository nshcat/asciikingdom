/*using System.Collections.Generic;
using Engine.Core;
using Engine.Graphics;
using Game.Serialization;
using Game.Simulation.Sites.Modules;

namespace Game.Simulation.Sites
{
    /// <summary>
    /// A village is a small settlement associated with a city which produces basic, raw resources and is
    /// taxed by its associated city.
    /// </summary>
    public class Village : WorldSite
    {
        #region Properties
        /// <summary>
        /// The city this village is associated with
        /// </summary>
        public City AssociatedCity { get; set; }

        /// <summary>
        /// Current site tile
        /// </summary>
        public override Tile Tile => Population.CurrentTile;

        /// <summary>
        /// Current site label
        /// </summary>
        public override string TypeDescriptor => Population.CurrentLabel;

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
            Name = name;
            Position = position;
            AssociatedCity = associatedCity;

            Population = new PopulationController(this, GrowthStages, initialPopulation);
            this.AddModule(Population);
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
                Id = Id,
                Name = Name,
                Population = Population.Population,
                Position = Position
            };
        }
    }
}*/