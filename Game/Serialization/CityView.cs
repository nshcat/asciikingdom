using System;
using System.Collections.Generic;
using System.Linq;
using Game.Simulation;

namespace Game.Serialization
{
    /// <summary>
    /// Serialization view for cities
    /// </summary>
    public class CityView : PopulatedSiteView
    {
        public List<VillageView> AssociatedVillages { get; set; }
        
        public City MakeObject()
        {
            var city = new City(this.Name, this.Position, this.Population);
            city.Id = this.Id;
            
            // Create all villages
            city.AssociatedVillages = this.AssociatedVillages
                .Select(x => x.MakeObject())
                .ToList();

            foreach (var village in city.AssociatedVillages)
                village.AssociatedCity = city;

            return city;
        }
    }
}