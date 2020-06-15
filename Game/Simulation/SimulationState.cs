using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Engine.Core;
using Game.Core;
using Game.Serialization;

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
        /// The current date in the simulated world
        /// </summary>
        public Date Date { get; set; }
        
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
            this.Date = new Date();
        }
        
        /// <summary>
        /// Update simulation state based on given number of elapsed weeks
        /// </summary>
        public void Update(int weeks)
        {
            this.Date.Weeks += weeks;
            
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

        /// <summary>
        /// Create a simulation view from this object
        /// </summary>
        public SimulationStateView ToView()
        {
            var provinces = this.Provinces.Select(x => x.ToView()).ToList();

            return new SimulationStateView
            {
                Provinces = provinces,
                Date = Date
            };
        }

        /// <summary>
        /// Save simulation state to disk, in given world folder
        /// </summary>
        public void Save(string prefix)
        {
            // Make sure the directory exists
            Directory.CreateDirectory(prefix);
            
            // First save the world
            this.World.Save(prefix);
            
            // Now create a view and serialize it
            var view = this.ToView();
            var statePath = Path.Combine(prefix, "state.json");
            File.WriteAllText(statePath, JsonSerializer.Serialize(view, Serialization.Serialization.DefaultOptions));
        }

        /// <summary>
        /// Load simulation state from given world directory prefix
        /// </summary>
        public static SimulationState Load(string prefix)
        {
            // First load world
            var world = World.Load(prefix);
            
            // Load serialization view
            var statePath = Path.Combine(prefix, "state.json");
            var view = JsonSerializer.Deserialize<SimulationStateView>(
                File.ReadAllText(statePath),
                Serialization.Serialization.DefaultOptions
            );

            return view.MakeObject(world);
        }
    }
}