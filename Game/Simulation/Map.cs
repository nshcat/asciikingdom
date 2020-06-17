using System;
using Engine.Core;
using Engine.Graphics;
using Game.Data;

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
        /// The terrain type array
        /// </summary>
        public TerrainType[,] Terrain { get; set; }
        
        /// <summary>
        /// Terrain discovery status
        /// </summary>
        public bool[,] Discovered { get; set; }
        
        /// <summary>
        /// Rendering of the map
        /// </summary>
        public Tile[,] Tiles { get; }
        
        /// <summary>
        /// Rendering of the rainfall map
        /// </summary>
        public Tile[,] Rainfall { get; set; }
        
        /// <summary>
        /// Rendering of the temperature map
        /// </summary>
        public Tile[,] Temperature { get; set; }
        
        /// <summary>
        /// Rendering of the drainage map
        /// </summary>
        public Tile[,] Drainage { get; set; }
        
        /// <summary>
        /// The seed used to generate this map
        /// </summary>
        public int Seed { get; }

        /// <summary>
        /// Construct a new, empty map with pre-allocated arrays.
        /// </summary>
        public Map(Size dimensions, int seed)
        {
            this.Dimensions = dimensions;
            this.Seed = seed;
            
            // TODO are these allocations needed? Doesn't the world generator just overwrite them?
            this.Terrain = new TerrainType[dimensions.Width, dimensions.Height];
            this.Tiles = new Tile[dimensions.Width, dimensions.Height];
            this.Rainfall = new Tile[dimensions.Width, dimensions.Height];
            this.Temperature = new Tile[dimensions.Width, dimensions.Height];
            this.Drainage = new Tile[dimensions.Width, dimensions.Height];
            this.Discovered = new bool[dimensions.Width, dimensions.Height];
        }
        
        /// <summary>
        /// Retrieves the terrain type at given position
        /// </summary>
        public TerrainType GetTerrainType(Position position)
        {
            return this.Terrain[position.X, position.Y];
        }

        /// <summary>
        /// Retrieves the terrain type info at given position
        /// </summary>
        public TerrainTypeInfo GetTerrainInfo(Position position)
        {
            return TerrainTypeData.GetInfo(this.GetTerrainType(position));
        }

        /// <summary>
        /// Build new tile array from terrain array
        /// </summary>
        public virtual void UpdateTiles()
        {
            var random = new Random(this.Seed);

            for (var ix = 0; ix < Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < Dimensions.Height; ++iy)
                {
                    var terrainType = this.GetTerrainType(new Position(ix, iy));

                    var info = TerrainTypeData.GetInfo(terrainType);
                    var tile = info.PickTile(random);
                    this.Tiles[ix, iy] = tile;
                }
            }
        }
    }
}