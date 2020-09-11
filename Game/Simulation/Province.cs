using System;
using System.Collections.Generic;
using System.Linq;
using Game.Maths;
using Game.Serialization;
using Game.Simulation.Modules;

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
        public City Capital
        {
            get => this._capital;
            set
            {
                // If there is already a capital associated with this province, we are about
                // to replace it - make sure we are not listening to its population changed event
                // anymore.
                if (this._capital != null)
                {
                    if (this._capital.HasModule<PopulationController>())
                    {
                        var controller = this._capital.QueryModule<PopulationController>();
                        controller.PopulationChanged -= this.OnPopulationChanged;
                    }
                }

                this._capital = value;

                if (this._capital.HasModule<PopulationController>())
                {
                    var controller = this._capital.QueryModule<PopulationController>();
                    controller.PopulationChanged += this.OnPopulationChanged;
                }
            }
        }

        /// <summary>
        /// Backing field for <see cref="Capital"/>
        /// </summary>
        private City _capital;
        
        /// <summary>
        /// Whether this probince can support an additional city
        /// </summary>
        public bool CanSupportNewCity => this.AssociatedCities.Count < this.CityCapacity;

        /// <summary>
        /// How many associated cities this province can manage
        /// </summary>
        public int CityCapacity { get; protected set; }
        
        /// <summary>
        /// This provinces influence radius. It determines where new cities can be built.
        /// </summary>
        public int InfluenceRadius { get; protected set; }

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

        /// <summary>
        /// Event handler for the population changed event of the capitals population
        /// controller
        /// </summary>
        protected void OnPopulationChanged(PopulationController source, int newPopulation)
        {
            this.RecalculateCityCapacity(source);
            this.RecalculateInfluenceRadius(source);
        }

        /// <summary>
        /// Recalculate the city capacity based on the current population value
        /// </summary>
        /// <param name="controller">The population controller instance</param>
        protected void RecalculateCityCapacity(PopulationController controller)
        {
            this.CityCapacity =
                3 + (int)(Math.Min(1.0f, controller.Population / 100000.0f)*7);
        }

        /// <summary>
        /// Recalculate the influenmce radius based on the current population value
        /// </summary>
        /// <param name="controller">The population controller instance</param>
        protected void RecalculateInfluenceRadius(PopulationController controller)
        {
            this.InfluenceRadius = (int)(
                (0.65f * Math.Min(1.0f, controller.Population / 100000.0f)        // How big the capital is, up to 100k
                 + 0.35f * Math.Min(1.0f, this.AssociatedCities.Count / 10.0f))   // How many cities there are, up to 10
                * 35 + 10                                                         // 10 is minimum, 45 is max
            );
        }
    }
}