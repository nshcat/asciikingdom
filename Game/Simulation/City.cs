using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Engine.Core;
using Engine.Graphics;
using Game.Maths;

namespace Game.Simulation
{
    /// <summary>
    /// A city is a settlement larger than a village, and the economical heart of a region.
    /// A city has a certain number of villages associated with it, from which it levies taxes
    /// in the form of raw resources.
    /// </summary>
    /// <remarks>
    /// The actual in-game title of a city depends on the population - they start out as towns, and will
    /// become cities when the population has grown beyond a certain number.
    /// </remarks>
    public class City : PopulatedSite
    {
        /// <summary>
        /// The name of this city
        /// </summary>
        public override string Name { get; set; }

        /// <summary>
        /// The position of this city on the world map
        /// </summary>
        public override Position Position { get; set; }
        
        /// <summary>
        /// The province this city is associated with
        /// </summary>
        public Province AssociatedProvince { get; set; }

        /// <summary>
        /// How many associated villages this city can support
        /// </summary>
        public int VillageCapacity => 3 + (this.Population / 35000);

        /// <summary>
        /// The radius of the influence sphere this city has. Associated villages
        /// can only be placed inside this sphere.
        /// </summary>
        public int InfluenceRadius => 5 + (this.Population / 20000);
        
        /// <summary>
        /// The current influence circle
        /// </summary>
        public Circle InfluenceCircle => new Circle(this.Position, this.InfluenceRadius);
        
        /// <summary>
        /// All villages associated with this city
        /// </summary>
        public List<Village> AssociatedVillages { get; }
            = new List<Village>();

        /// <summary>
        /// List of city growth stages
        /// </summary>
        private static List<SiteGrowthStage> GrowthStages { get; } = new List<SiteGrowthStage>
        {
            new SiteGrowthStage(0, "Small Town", new Tile(43, Color.FromHex("#CDD2D2"))),
            new SiteGrowthStage(15000, "Town", new Tile(43, Color.FromHex("#CDD2D2"))),
            new SiteGrowthStage(45000, "Big Town", new Tile(43, Color.FromHex("#CDD2D2"))),
            new SiteGrowthStage(65000, "Small City", new Tile(35, Color.FromHex("#CDD2D2"))),
            new SiteGrowthStage(85000, "City", new Tile(35, Color.FromHex("#CDD2D2"))),
            new SiteGrowthStage(100000, "Big City", new Tile(15, Color.FromHex("#CDD2D2")))
        };
        
        /// <summary>
        /// Create new city with given name
        /// </summary>
        public City(string name, Position position, int population) : base(GrowthStages, population)
        {
            this.Name = name;
            this.Position = position;
        }

        /// <summary>
        /// Create new city with empty name, position and population
        /// </summary>
        public City() : this("", Position.Origin, 0)
        {
            
        }

        /// <summary>
        /// Update this cities simulation state.
        /// </summary>
        public override void Update(int weeks)
        {
            foreach (var village in this.AssociatedVillages)
            {
                village.Update(weeks);
            }
        }
    }
}