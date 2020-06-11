using System;
using System.Collections.Generic;

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
        /// Create a new province with given capital
        /// </summary>
        public Province(City capital)
        {
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
    }
}