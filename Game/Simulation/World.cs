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

namespace Game.Simulation
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
        public Size OverviewDimensions => this.Dimensions * this.OverviewScale;

        /// <summary>
        /// The relative scale of the overview map in relation to the detailed map
        /// </summary>
        public float OverviewScale => this.Metadata.OverviewScale;

        /// <summary>
        /// The seed used to generate the current map
        /// </summary>
        public int Seed => this.Metadata.Seed;

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
            this.Metadata = metadata;
            this.DetailedMap = new DetailedMap(this.Dimensions, this.Seed);
            this.OverviewMap = new Map(this.OverviewDimensions, this.Seed);
        }

        /// <summary>
        /// Construct new, empty world instance
        /// </summary>
        public World(Size dimensions, int seed, float overviewScale = 0.1250f)
            : this(new WorldMetadata(dimensions, seed, overviewScale))
        {
            
        }

        /// <summary>
        /// Update the stored tiles to be up-to-date with the current terrain type map
        /// </summary>
        public void UpdateTiles()
        {
            this.DetailedMap.UpdateTiles();
            
            // Since the terrain might have changes, we have to update the overview map as well.
            // This will also cause a call to UpdateTiles in the overview map instance.
            this.BuildOverviewTerrain();
        }

        /// <summary>
        /// Build the overview map based on the detailed map
        /// </summary>
        public void BuildOverview()
        {
            this.BuildOverviewTerrain();
            this.BuildOverviewRainfall();
            this.BuildOverviewTemperature();
            this.BuildOverviewDrainage();
            this.BuildOverviewDiscovered();
            this.CalcOverviewRawValues();
        }
        
        /// <summary>
        /// Build overview map terrain based on the detailed map
        /// </summary>
        protected void BuildOverviewTerrain()
        {
            this.BuildOverviewHelper(
                this.DetailedMap.Terrain,
                this.OverviewMap.Terrain,
                new HashSet<TerrainType> {TerrainType.River}); // Exclude rivers, since they are special map elements
            
            // Derive tiles from terrain types
            this.OverviewMap.UpdateTiles();
        }
        
        /// <summary>
        /// Build overview map rainfall map based on the detailed map
        /// </summary>
        protected void BuildOverviewRainfall()
        {
            this.BuildOverviewHelper(this.DetailedMap.Rainfall, this.OverviewMap.Rainfall);
        }
        
        /// <summary>
        /// Build overview map raw value array based on detailed map
        /// </summary>
        protected void CalcOverviewRawValues()
        {
            this.BuildOverviewHelper(this.DetailedMap.RawDrainage, this.OverviewMap.RawDrainage);
            this.BuildOverviewHelper(this.DetailedMap.RawRainfall, this.OverviewMap.RawRainfall);
            this.BuildOverviewHelper(this.DetailedMap.RawTemperature, this.OverviewMap.RawTemperature);
        }

        /// <summary>
        /// Build overview map drainage map based on the detailed map
        /// </summary>
        protected void BuildOverviewDrainage()
        {
            this.BuildOverviewHelper(this.DetailedMap.Drainage, this.OverviewMap.Drainage);
        }
        
        /// <summary>
        /// Build overview map temperature map based on the detailed map
        /// </summary>
        protected void BuildOverviewTemperature()
        {
            this.BuildOverviewHelper(this.DetailedMap.Temperature, this.OverviewMap.Temperature);
        }

        /// <summary>
        /// Build discovery map based on detailed map
        /// </summary>
        protected void BuildOverviewDiscovered()
        {
            this.BuildOverviewHelper(this.DetailedMap.Discovered, this.OverviewMap.Discovered);
        }

        /// <summary>
        /// A helper method used to scale down arrays of detailed map to be used with the overview map
        /// </summary>
        protected void BuildOverviewHelper<T>(T[,] source, T[,] destination, HashSet<T> exclude = null)
        {
            var entries = new List<T>();
            var scaleFactor = (int)(1.0f / this.OverviewScale);

            if (exclude == null)
                exclude = new HashSet<T>();
            
            for (var ix = 0; ix < this.OverviewDimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.OverviewDimensions.Height; ++iy)
                {
                    var topLeft = new Position(ix * scaleFactor, iy * scaleFactor);
                    var bottomRight = new Position(topLeft.X + 4, topLeft.Y + 4);

                    entries.Clear();
                    
                    for (var iix = topLeft.X; iix <= bottomRight.X; ++iix)
                    {
                        for (var iiy = topLeft.Y; iiy <= bottomRight.Y; ++iiy)
                        {
                            var element = source[iix, iiy];
                            
                            if(!exclude.Contains(element))
                                entries.Add(source[iix, iiy]);
                        }
                    }
                    
                    var average = entries
                        .GroupBy(x => x)
                        .Select(x => new
                        {
                            Count = x.Count(),
                            Entry = x.Key
                        })
                        .OrderByDescending(x => x.Count)
                        .Select(x => x.Entry)
                        .First();

                    destination[ix, iy] = average;
                }
            }
        }

        /// <summary>
        /// Save the world to given world directory path
        /// </summary>
        /// <param name="prefix">The directory in which the world will be saved</param>
        public void Save(string prefix)
        {
            // Serialize meta data to own JSON file
            var metadataPath = Path.Combine(prefix, "metadata.json");
            Serialization.Serialization.SerializeToFile(this.Metadata, metadataPath, Serialization.Serialization.DefaultOptions);

            // Write terrain data
            var terrainPath = Path.Combine(prefix, "terrain.bin");
            using (var writer = new BinaryWriter(File.Open(terrainPath, FileMode.Create)))
            {
                for (var ix = 0; ix < this.Dimensions.Width; ++ix)
                {
                    for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                    {
                        var value = (byte) this.DetailedMap.Terrain[ix, iy];
                        writer.Write(value);
                    }
                }
            }

            // Write raw values
            var rawPath = Path.Combine(prefix, "raw.bin");
            using (var writer = new BinaryWriter(File.Open(rawPath, FileMode.Create)))
            {
                for (var ix = 0; ix < this.Dimensions.Width; ++ix)
                {
                    for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                    {
                        writer.Write(this.DetailedMap.RawDrainage[ix, iy]);
                        writer.Write(this.DetailedMap.RawRainfall[ix, iy]);
                        writer.Write(this.DetailedMap.RawTemperature[ix, iy]);
                    }
                }
            }

            // Write discovery data
            var discoveryPath = Path.Combine(prefix, "discovery.bin");
            using (var writer = new BinaryWriter(File.Open(discoveryPath, FileMode.Create)))
            {
                for (var ix = 0; ix < this.Dimensions.Width; ++ix)
                {
                    for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                    {
                        var value = (byte) (this.DetailedMap.Discovered[ix, iy] ? 1 : 0);
                        writer.Write(value);
                    }
                }
            }
            
            // Write extra river data
            var riverPath = Path.Combine(prefix, "rivers.bin");
            using (var writer = new BinaryWriter(File.Open(riverPath, FileMode.Create)))
            {
                writer.Write(this.DetailedMap.RiverTileInfo.Count);
                
                foreach (var entry in this.DetailedMap.RiverTileInfo)
                {
                    writer.Write(entry.Key.X);
                    writer.Write(entry.Key.Y);
                    writer.Write(entry.Value.Size);
                    writer.Write((byte)entry.Value.Type);
                }
            }

            // Save resources
            var resourcePath = Path.Combine(prefix, "resources.json");
            var resources = this.DetailedMap.Resources
                .ToDictionary(x => x.Key.ToString(), x => x.Value.Identifier);
            
            File.WriteAllText(resourcePath, JsonSerializer.Serialize(resources, Serialization.Serialization.DefaultOptions));
        }

        /// <summary>
        /// Load world from given directory
        /// </summary>
        public static World Load(string prefix)
        {
            // Load metadata first
            var metadataPath = Path.Combine(prefix, "metadata.json");
            var metadata = Serialization.Serialization.DeserializeFromFile<WorldMetadata>(metadataPath, Serialization.Serialization.DefaultOptions);
            
            var world = new World(metadata);
            
            // Load terrain
            var terrainPath = Path.Combine(prefix, "terrain.bin");
            using (var reader = new BinaryReader(File.Open(terrainPath, FileMode.Open)))
            {
                for (var ix = 0; ix < metadata.Dimensions.Width; ++ix)
                {
                    for (var iy = 0; iy < metadata.Dimensions.Height; ++iy)
                    {
                        var value = (TerrainType)reader.ReadByte();
                        world.DetailedMap.Terrain[ix, iy] = value;
                    }
                }
            }

            // Load raw values
            var rawPath = Path.Combine(prefix, "raw.bin");
            using (var reader = new BinaryReader(File.Open(rawPath, FileMode.Open)))
            {
                for (var ix = 0; ix < metadata.Dimensions.Width; ++ix)
                {
                    for (var iy = 0; iy < metadata.Dimensions.Height; ++iy)
                    {
                        world.DetailedMap.RawDrainage[ix, iy] = reader.ReadSingle();
                        world.DetailedMap.RawRainfall[ix, iy] = reader.ReadSingle();
                        world.DetailedMap.RawTemperature[ix, iy] = reader.ReadSingle();
                    }
                }
            }

            // Load discovery state
            var discoveryPath = Path.Combine(prefix, "discovery.bin");
            using (var reader = new BinaryReader(File.Open(discoveryPath, FileMode.Open)))
            {
                for (var ix = 0; ix < metadata.Dimensions.Width; ++ix)
                {
                    for (var iy = 0; iy < metadata.Dimensions.Height; ++iy)
                    {
                        var value = reader.ReadByte() == 1;
                        world.DetailedMap.Discovered[ix, iy] = value;
                    }
                }
            }

            // Load river info
            var riverPath = Path.Combine(prefix, "rivers.bin");
            using (var reader = new BinaryReader(File.Open(riverPath, FileMode.Open)))
            {
                // Read number of entries
                var count = reader.ReadInt32();

                world.DetailedMap.RiverTileInfo = new Dictionary<Position, RiverTileInfo>();
                
                for (var i = 0; i < count; ++i)
                {
                    var x = reader.ReadInt32();
                    var y = reader.ReadInt32();
                    var size = reader.ReadInt32();
                    var type = (RiverTileType) reader.ReadByte();
                    
                    world.DetailedMap.RiverTileInfo.Add(new Position(x, y), new RiverTileInfo(type, size));
                }
            }
            
            // Load resource info
            var resourcePath = Path.Combine(prefix, "resources.json");
            var resources = JsonSerializer.Deserialize<Dictionary<string, string>>(
                File.ReadAllText(resourcePath),
                Serialization.Serialization.DefaultOptions
            );

            world.DetailedMap.Resources = resources.ToDictionary(
                x => Position.FromString(x.Key),
                x => ResourceTypeManager.Instance.GetType(x.Value)
            );
            
            world.UpdateTiles();

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

            for (var iy = 0; iy < this.Dimensions.Height / 3; ++iy)
            {
                for (var ix = 0; ix < this.Dimensions.Width; ++ix)
                {
                    points.Add(new Position(ix, iy));
                }
            }
            
            points.Shuffle(rng);
            
            foreach(var position in points)
            {
                if (this.IsLand(position))
                {
                    this.DetailedMap.Discovered = new bool[this.Dimensions.Width, this.Dimensions.Height];
                    var continent = this.DiscoverContinent(position);

                    if (continent.Count > 400) 
                    {
                        // Save seed position
                        this.Metadata.InitialLocation = position;
                        
                        // Discover a bit more of the ocean
                        for (var ix = 0; ix < 3; ++ix)
                        {
                            var shore = new HashSet<Position>();
                            
                            foreach (var pos in continent)
                            {
                                if (!this.IsLand(pos))
                                {
                                    foreach (var direction in Directions)
                                    {
                                        var newPos = pos + direction;

                                        if (!this.InsideWorldBounds(newPos))
                                            continue;
                                        
                                        if (!this.IsLand(newPos))
                                        {
                                            shore.Add(newPos);
                                            this.DetailedMap.Discovered[newPos.X, newPos.Y] = true;
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
            this.DetailedMap.Discovered[seed.X, seed.Y] = true;

            while (toVisit.Count > 0)
            {
                var next = toVisit.Dequeue();
                visited.Add(next);
                
                foreach (var direction in Directions)
                {
                    var neighbour = next + direction;
                    
                    if(!this.InsideWorldBounds(neighbour))
                        continue;

                    this.DetailedMap.Discovered[neighbour.X, neighbour.Y] = true;

                    if(this.IsLand(neighbour) && !visited.Contains(neighbour) && !toVisit.Contains(neighbour))
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
            var terrainType = this.DetailedMap.Terrain[position.X, position.Y];

            return terrainType != TerrainType.Ocean && terrainType != TerrainType.SeaIce && terrainType != TerrainType.Glacier;
        }

        /// <summary>
        /// Check whether given position lies within world bounds
        /// </summary>
        protected bool InsideWorldBounds(Position position)
        {
            return position.X > 0
                   && position.X < this.Dimensions.Width
                   && position.Y > 0
                   && position.Y < this.Dimensions.Height;
        }
    }
}