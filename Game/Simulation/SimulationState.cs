using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Nodes;
using Engine.Core;
using Game.Core;
using Game.Simulation.Sites;

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
            = new Date();

        /// <summary>
        /// Manager instance that contains all sites in the current world
        /// </summary>
        public SiteManager Sites { get; protected set; }
            = new SiteManager();

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
            this.Date.Weeks += weeks;

            this.Sites.Update(weeks);
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

            // Now serialize remaining state data to JSON tree
            var root = new JsonObject();
            root.Add("date", JsonSerializer.SerializeToNode(this.Date));
            root.Add("sites", this.Sites.Serialize());

            // Write JSON tree to disk
            var statePath = Path.Combine(prefix, "state.json");
            File.WriteAllText(statePath, root.ToJsonString(Serialization.Serialization.DefaultOptions));
        }

        /// <summary>
        /// Load simulation state from given world directory prefix
        /// </summary>
        public static SimulationState Load(string prefix)
        {
            // First load world
            var world = World.Load(prefix);
            var state = new SimulationState(world);

            // Load JSON data
            var statePath = Path.Combine(prefix, "state.json");
            var root = JsonNode.Parse(File.ReadAllText(statePath)).AsObject();
            state.Date = JsonSerializer.Deserialize<Date>(root["date"]);
            state.Sites.Deserialize(root["sites"]);

            return state;
        }
    }
}