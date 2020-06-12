using System;
using System.Collections.Generic;
using Engine.Core;
using Game.Core;

namespace Game.Simulation
{
    /// <summary>
    /// Represents the current state of the simulation. Contains both the world and all entity classes of the game
    /// object graph.
    /// </summary>
    public class SimulationState
    {
        /// <summary>
        /// The game world
        /// </summary>
        public World World { get; set; }
        
        /// <summary>
        /// All provinces of the kingdom
        /// </summary>
        public List<Province> Provinces { get; set; }
            = new List<Province>();

        /// <summary>
        /// Create a new simulation state representing a fresh game world, using given world.
        /// </summary>
        public SimulationState(World world)
        {
            this.World = world;
        }
        
        /// <summary>
        /// Update simulation state based on given number of elapsed weeks
        /// </summary>
        public void Update(int weeks)
        {
            foreach(var province in this.Provinces)
                province.Update(weeks);
        }
        
        /// <summary>
        /// Retrieve all sites on the world map
        /// </summary>
        public Dictionary<Position, IWorldSite> GetAllSites()
        {
            var result = new Dictionary<Position, IWorldSite>();

            foreach (var province in this.Provinces)
            {
                foreach (var city in province.AssociatedCities)
                {
                    result.Add(city.Position, city);

                    foreach (var village in city.AssociatedVillages)
                    {
                        result.Add(village.Position, village);
                    }
                }
            }

            return result;
        }
    }
}