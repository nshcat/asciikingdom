using System;
using System.Collections.Generic;
using Engine.Core;
using Engine.Graphics;
using Game.Data;
using Game.Utility;

namespace Game.Simulation
{
    /// <summary>
    /// Represents a two-dimensional array of terrain type instances, which make up a world map.
    /// </summary>
    /// <remarks>
    /// The <see cref="World"/> class uses two maps: One for the detailed play map, and one for the smaller overview
    /// map.
    /// </remarks>
    public class Map
    {
        /// <summary>
        /// Dimensions of the map, in tiles
        /// </summary>
        public Size Dimensions { get; }

        /// <summary>
        /// The seed used to generate this map
        /// </summary>
        public int Seed { get; }

        /// <summary>
        /// Terrain discovery status
        /// </summary>
        public DiscoveryMap Discovered { get; set; }

        /// <summary>
        /// The main terrain world layer
        /// </summary>
        public TerrainWorldLayer TerrainLayer { get; set; }

        /// <summary>
        /// The main terrain tile layer
        /// </summary>
        public TileWorldLayer TerrainTileLayer { get; set; }

        /// <summary>
        /// Dictionary of all additional world layers available for this map
        /// </summary>
        public Dictionary<string, WorldLayer> Layers { get; set; }
            = new Dictionary<string, WorldLayer>();


        /// <summary>
        /// Construct a new, empty map with pre-allocated arrays.
        /// </summary>
        public Map(Size dimensions, int seed)
        {
            this.Dimensions = dimensions;
            this.Seed = seed;

            this.Discovered = new DiscoveryMap(dimensions);
        }

        /// <summary>
        /// Retrieve layer with given id, casted down to given world layer type
        /// </summary>
        public T GetLayer<T>(string layerid) where T : WorldLayer
        {
            if (!this.Layers.ContainsKey(layerid))
                throw new ArgumentException($"Map doesnt contain layer with id {layerid}");

            return this.Layers[layerid].As<T>();
        }

        /// <summary>
        /// Retrieves the terrain type at given position
        /// </summary>
        public TerrainType GetTerrainType(Position position)
        {
            return this.TerrainLayer.Values[position.X, position.Y];
        }

        /// <summary>
        /// Retrieves the terrain type info at given position
        /// </summary>
        public TerrainTypeInfo GetTerrainInfo(Position position)
        {
            return TerrainTypeData.GetInfo(this.GetTerrainType(position));
        }

        /// <summary>
        /// Check if given map cell is discovered, i.e. lies outside the fog of war
        /// </summary>
        public bool IsDiscovered(Position position)
        {
            return this.Discovered.Values[position.X, position.Y];
        }

        /// <summary>
        /// Create as overview map from given detailed map
        /// </summary>
        public void InitializeFromDetailed(DetailedMap map, float factor)
        {
            this.TerrainLayer = map.TerrainLayer.CreateOverview(factor).As<TerrainWorldLayer>();
            this.TerrainTileLayer = TerrainTileLayerGenerator.CreateTileLayer(this.Seed, this.TerrainLayer);

            this.Layers = new Dictionary<string, WorldLayer>();
            foreach(var kvp in map.Layers)
            {
                this.Layers.Add(kvp.Key, kvp.Value.CreateOverview(factor));
            }

            this.Discovered = map.Discovered.CreateOverview(factor);
        }  
    }
}