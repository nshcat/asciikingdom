using System;
using System.Collections.Generic;
using System.Linq;
using Game.Simulation;

namespace Game.Serialization
{
    /// <summary>
    /// Serialization view for provinces
    /// </summary>
    public class ProvinceView : GameObjectView
    {
        public List<CityView> AssociatedCities { get; set; }
        
        public string Name { get; set; }

        public Guid Capital { get; set; }
        
        public Province MakeObject()
        {
            var cities = this.AssociatedCities.Select(x => x.MakeObject()).ToList();

            var capital = cities.First(x => x.Id == this.Capital);

            var province = new Province(this.Name, capital);
            province.AssociatedCities = cities;
            province.Id = this.Id;

            foreach (var city in cities)
            {
                city.AssociatedProvince = province;
            }

            return province;
        }
    }
}