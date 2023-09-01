using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Engine.Core;
using Game.Data;
using Game.Utility;
using OpenToolkit.Graphics.OpenGL;
using Game.Serialization;

namespace Game.Simulation.Worlds
{
    /// <summary>
    /// Represents the game world.
    /// </summary>
    public class World
    {
        /// <summary>
        /// The index of the world in the save directory. If set to -1, the world has not been saved yet.
        /// </summary>
        public int Index { get; set; } = -1;

        /// <summary>
        /// Various data about the world
        /// </summary>
        public WorldMetadata Metadata { get; protected set; }

        /// <summary>
        /// The size of the map, in terrain tiles.
        /// </summary>
        public Size Dimensions => Metadata.Dimensions;

        /// <summary>
        /// The size of the overview map, in terrain tiles.
        /// </summary>
        public Size OverviewDimensions => Dimensions * OverviewScale;

        /// <summary>
        /// The relative scale of the overview map in relation to the detailed map
        /// </summary>
        public float OverviewScale => Metadata.OverviewScale;

        /// <summary>
        /// The seed used to generate the current map
        /// </summary>
        public int Seed => Metadata.Seed;

        /// <summary>
        /// The detailed play map
        /// </summary>
        public DetailedMap DetailedMap { get; }

        /// <summary>
        /// The overview map
        /// </summary>
        public Map OverviewMap { get; }

        /// <summary>
        /// Construct new, empty world instance
        /// </summary>
        public World(WorldMetadata metadata)
        {
            Metadata = metadata;
            DetailedMap = new DetailedMap(Dimensions, Seed);
            OverviewMap = new Map(OverviewDimensions, Seed);
        }

        /// <summary>
        /// Construct new, empty world instance
        /// </summary>
        public World(Size dimensions, int seed, float overviewScale = 0.1250f)
            : this(new WorldMetadata(dimensions, seed, overviewScale))
        {

        }

        /// <summary>
        /// Save the world to given world directory path
        /// </summary>
        /// <param name="prefix">The directory in which the world will be saved</param>
        public void Save(string prefix)
        {
            // Serialize meta data to own JSON file
            var metadataPath = Path.Combine(prefix, "metadata.json");
            Serialization.Serialization.SerializeToFile(Metadata, metadataPath, Serialization.Serialization.DefaultOptions);

            // Write detailed map data
            DetailedMap.Save(prefix);
        }

        /// <summary>
        /// Load world from given directory
        /// </summary>
        public static World Load(string prefix)
        {
            // Load metadata first
            var metadataPath = Path.Combine(prefix, "metadata.json");
            var metadata = Serialization.Serialization.DeserializeFromFile<WorldMetadata>(metadataPath, Serialization.Serialization.DefaultOptions);

            // Create world based on given metadata
            var world = new World(metadata);

            // Now load the detailed map
            world.DetailedMap.Load(prefix);

            // Create overview map as smaller copy of detailed map
            world.OverviewMap.InitializeFromDetailed(world.DetailedMap, world.OverviewScale);

            return world;
        }

        /// <summary>
        /// Find and uncover initial continent for game start
        /// </summary>
        public void DiscoverInitialContinent()
        {
            var rng = new Random();

            int tries = 0;

            var points = new List<Position>();

            for (var iy = 0; iy < Dimensions.Height / 3; ++iy)
            {
                for (var ix = 0; ix < Dimensions.Width; ++ix)
                {
                    points.Add(new Position(ix, iy));
                }
            }

            points.Shuffle(rng);

            foreach (var position in points)
            {
                if (IsLand(position))
                {
                    DetailedMap.Discovered = new DiscoveryMap(Dimensions);
                    var continent = DiscoverContinent(position);

                    if (continent.Count > 400)
                    {
                        // Save seed position
                        Metadata.InitialLocation = position;

                        // Discover a bit more of the ocean
                        for (var ix = 0; ix < 3; ++ix)
                        {
                            var shore = new HashSet<Position>();

                            foreach (var pos in continent)
                            {
                                if (!IsLand(pos))
                                {
                                    foreach (var direction in Directions)
                                    {
                                        var newPos = pos + direction;

                                        if (!InsideWorldBounds(newPos))
                                            continue;

                                        if (!IsLand(newPos))
                                        {
                                            shore.Add(newPos);
                                            DetailedMap.Discovered.Values[newPos.X, newPos.Y] = true;
                                        }
                                    }
                                }
                            }

                            continent = shore;
                        }

                        return;
                    }
                }
            }

            throw new Exception("Could not find big enough start continent");
        }

        /// <summary>
        /// The four cardinal directions
        /// </summary>
        protected static readonly List<Position> Directions = new List<Position>
        {
            new Position(1, 0),
            new Position(0, 1),
            new Position(-1, 0),
            new Position(0, -1)
        };

        /// <summary>
        /// Discover the continent the given seed position lies within and return the total continent size, in tiles
        /// </summary>
        protected HashSet<Position> DiscoverContinent(Position seed)
        {
            var visited = new HashSet<Position>();
            var toVisit = new Queue<Position>();
            toVisit.Enqueue(seed);
            DetailedMap.Discovered.Values[seed.X, seed.Y] = true;

            while (toVisit.Count > 0)
            {
                var next = toVisit.Dequeue();
                visited.Add(next);

                foreach (var direction in Directions)
                {
                    var neighbour = next + direction;

                    if (!InsideWorldBounds(neighbour))
                        continue;

                    DetailedMap.Discovered.Values[neighbour.X, neighbour.Y] = true;

                    if (IsLand(neighbour) && !visited.Contains(neighbour) && !toVisit.Contains(neighbour))
                        toVisit.Enqueue(neighbour);

                    visited.Add(neighbour);
                }
            }

            return visited;
        }

        /// <summary>
        /// Whether the terrain at given position counts as land
        /// </summary>
        protected bool IsLand(Position position)
        {
            var terrainType = DetailedMap.TerrainLayer.Values[position.X, position.Y];

            return terrainType != TerrainType.Ocean && terrainType != TerrainType.SeaIce && terrainType != TerrainType.Glacier;
        }

        /// <summary>
        /// Check whether given position lies within world bounds
        /// </summary>
        protected bool InsideWorldBounds(Position position)
        {
            return position.X > 0
                   && position.X < Dimensions.Width
                   && position.Y > 0
                   && position.Y < Dimensions.Height;
        }
    }
}