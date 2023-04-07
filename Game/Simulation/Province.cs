/*using System;
using System.Collections.Generic;
using System.Linq;
using Game.Maths;
using Game.Simulation.Sites.Modules;

namespace Game.Simulation.Sites
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
            get => _capital;
            set
            {
                // If there is already a capital associated with this province, we are about
                // to replace it - make sure we are not listening to its population changed event
                // anymore.
                if (_capital != null)
                {
                    if (_capital.HasModule<PopulationController>())
                    {
                        var controller = _capital.QueryModule<PopulationController>();
                        controller.PopulationChanged -= OnPopulationChanged;
                    }
                }

                _capital = value;

                if (_capital.HasModule<PopulationController>())
                {
                    var controller = _capital.QueryModule<PopulationController>();
                    controller.PopulationChanged += OnPopulationChanged;
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
        public bool CanSupportNewCity => AssociatedCities.Count < CityCapacity;

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
        public Circle InfluenceCircle => new Circle(Capital.Position, InfluenceRadius);

        /// <summary>
        /// Create a new province with given capital
        /// </summary>
        public Province(string name, City capital)
        {
            Name = name;
            AssociatedCities.Add(capital);
            Capital = capital;
            capital.AssociatedProvince = this;
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Update the simulation state of this province.
        /// </summary>
        public void Update(int weeks)
        {
            foreach (var city in AssociatedCities)
                city.Update(weeks);
        }

        /// <summary>
        /// Create a simulation view from this object
        /// </summary>
        public ProvinceView ToView()
        {
            var cities = AssociatedCities.Select(x => x.ToView()).ToList();

            return new ProvinceView
            {
                Id = Id,
                Name = Name,
                Capital = Capital.Id,
                AssociatedCities = cities
            };
        }

        /// <summary>
        /// Event handler for the population changed event of the capitals population
        /// controller
        /// </summary>
        protected void OnPopulationChanged(PopulationController source, int newPopulation)
        {
            RecalculateCityCapacity(source);
            RecalculateInfluenceRadius(source);
        }

        /// <summary>
        /// Recalculate the city capacity based on the current population value
        /// </summary>
        /// <param name="controller">The population controller instance</param>
        protected void RecalculateCityCapacity(PopulationController controller)
        {
            CityCapacity =
                3 + (int)(Math.Min(1.0f, controller.Population / 100000.0f) * 7);
        }

        /// <summary>
        /// Recalculate the influenmce radius based on the current population value
        /// </summary>
        /// <param name="controller">The population controller instance</param>
        protected void RecalculateInfluenceRadius(PopulationController controller)
        {
            InfluenceRadius = (int)(
                (0.65f * Math.Min(1.0f, controller.Population / 100000.0f)        // How big the capital is, up to 100k
                 + 0.35f * Math.Min(1.0f, AssociatedCities.Count / 10.0f))   // How many cities there are, up to 10
                * 35 + 10                                                         // 10 is minimum, 45 is max
            );
        }
    }
}*/