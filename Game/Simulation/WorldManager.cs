using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Game.Core;

namespace Game.Simulation
{
    /// <summary>
    /// Class managing the loading and saving of worlds.
    /// </summary>
    public class WorldManager
    {
        /// <summary>
        /// List of all known worlds, specified by their index and name
        /// </summary>
        public List<(int, string)> Worlds { get; set; }

        /// <summary>
        /// Whether there are any saved worlds available
        /// </summary>
        public bool HasWorlds => this.Worlds.Count > 0;

        /// <summary>
        /// Refresh list of known worlds
        /// </summary>
        public void RefreshWorlds()
        {
            this.Worlds = new List<(int, string)>();

            foreach (var directory in Directory.GetDirectories(GameDirectories.SaveGames))
            {
                var name = Path.GetDirectoryName(directory);
                
                if (name.StartsWith("world"))
                {
                    var index = int.Parse(name.Replace("world", ""));

                    var metadata = Serialization.Serialization.DeserializeFromFile<WorldMetadata>(
                        Path.Combine(directory, "metadata.json"), Serialization.Serialization.DefaultOptions);
                    
                    this.Worlds.Add((index, metadata.Name));
                }
            }
        }

        /// <summary>
        /// Save the given world to disk.
        /// </summary>
        public void SaveWorld(World world)
        {
            // Check if it hasnt already been saved before and thus lacks an index
            if (world.Index == -1)
            {
                world.Index = this.GetNextWorldIndex();
            }
            
            world.Save(this.BuildPrefix(world.Index));
        }

        /// <summary>
        /// Load world with given index
        /// </summary>
        public World LoadWorld(int index)
        {
            return World.Load(this.BuildPrefix(index));
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
            var index = 0;

            while (Directory.Exists(this.BuildPrefix(index)))
            {
                ++index;
            }

            return index;
        }
    }
}