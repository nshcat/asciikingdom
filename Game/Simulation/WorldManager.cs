using System.Collections.Generic;
using System.IO;
using Game.Core;
using Game.Serialization;

namespace Game.Simulation
{
    /// <summary>
    /// Class managing the loading and saving of worlds.
    /// </summary>
    public class WorldManager
    {
        /// <summary>
        /// The singleton instance
        /// </summary>
        private static WorldManager _instance;

        /// <summary>
        /// The global world manager instance
        /// </summary>
        public static WorldManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new WorldManager();

                return _instance;
            }
        }

        /// <summary>
        /// Disallow construction from outside
        /// </summary>
        private WorldManager()
        {

        }

        /// <summary>
        /// List of all known worlds, specified by their index and name
        /// </summary>
        public List<(int, string)> Worlds { get; set; }

        /// <summary>
        /// Whether there are any saved worlds available
        /// </summary>
        public bool HasWorlds => Worlds.Count > 0;

        /// <summary>
        /// Refresh list of known worlds
        /// </summary>
        public void RefreshWorlds()
        {
            Worlds = new List<(int, string)>();

            foreach (var directory in Directory.GetDirectories(GameDirectories.SaveGames))
            {
                var name = Path.GetFileName(directory);

                if (name.StartsWith("world"))
                {
                    var index = int.Parse(name.Replace("world", ""));

                    var metadata = Serialization.Serialization.DeserializeFromFile<WorldMetadata>(
                        Path.Combine(directory, "metadata.json"), Serialization.Serialization.DefaultOptions);

                    Worlds.Add((index, metadata.Name));
                }
            }
        }

        /// <summary>
        /// Save the given world to disk.
        /// </summary>
        public void SaveWorld(SimulationState state)
        {
            // Check if it hasn't already been saved before and thus lacks an index
            if (state.World.Index == -1)
            {
                state.World.Index = GetNextWorldIndex();
            }

            state.Save(this.BuildPrefix(state.World.Index));
        }

        /// <summary>
        /// Load world with given index
        /// </summary>
        public SimulationState LoadWorld(int index)
        {
            var state = SimulationState.Load(BuildPrefix(index));
            state.World.Index = index;
            return state;
        }

        /// <summary>
        /// Build world save directory path
        /// </summary>
        protected string BuildPrefix(int index) => Path.Combine(GameDirectories.SaveGames, $"world{index}");

        /// <summary>
        /// Retrieve the next free world index
        /// </summary>
        /// <remarks>
        /// This checks the world save directory for a free index that does not exist
        /// </remarks>
        protected int GetNextWorldIndex()
        {
            var index = 1;

            while (Directory.Exists(BuildPrefix(index)))
            {
                ++index;
            }

            return index;
        }
    }
}