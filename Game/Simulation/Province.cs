using System;
using System.Collections.Generic;
using System.Linq;
using Game.Maths;
using Game.Serialization;

namespace Game.Simulation
{
    /// <summary>
    /// A province is a part of a kingdom, and governs a number of cities and their villages.
    /// Each province has a province capital.
    /// </summary>
    public class Province : IGameObject
    {
        /// <summary>
        /// Unique identifier of this province
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Name of the province
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// All cities associated with this province
        /// </summary>
        public List<City> AssociatedCities { get; set; }
            = new List<City>();
        
        /// <summary>
        /// The province capital.
        /// </summary>
        /// <remarks>
        /// This city object is also present in the <see cref="AssociatedCities"/> collection.
        /// </remarks>
        public City Capital { get; set; }
        
        /// <summary>
        /// This provinces influence radius. It determines where new cities can be built.
        /// </summary>
        public int InfluenceRadius => (int)(
                (0.65f * (this.Capital.Population / 100000.0f)     // How big the capital is, up to 100k
                + 0.35f * (this.AssociatedCities.Count / 10.0f))   // How many cities there are, up to 10
                * 35 + 10                                          // 10 is minimum, 45 is max
            );

        /// <summary>
        /// The circle representing this provinces influence radius
        /// </summary>
        public Circle InfluenceCircle => new Circle(this.Capital.Position, this.InfluenceRadius);
        
        /// <summary>
        /// Create a new province with given capital
        /// </summary>
        public Province(string name, City capital)
        {
            this.Name = name;
            this.AssociatedCities.Add(capital);
            this.Capital = capital;
            capital.AssociatedProvince = this;
            this.Id = Guid.NewGuid();
        }
        
        /// <summary>
        /// Update the simulation state of this province.
        /// </summary>
        public void Update(int weeks)
        {
            foreach(var city in this.AssociatedCities)
                city.Update(weeks);
        }
        
        /// <summary>
        /// Create a simulation view from this object
        /// </summary>
        public ProvinceView ToView()
        {
            var cities = this.AssociatedCities.Select(x => x.ToView()).ToList();

            return new ProvinceView
            {
                Id = this.Id,
                Name = this.Name,
                Capital = this.Capital.Id,
                AssociatedCities = cities
            };
        }
    }
}