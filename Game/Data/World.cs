using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Core;
using Engine.Graphics;

namespace Game.Data
{
    /// <summary>
    /// Represents the game world.
    /// </summary>
    public partial class World
    {
        /// <summary>
        /// The size of the map, in terrain tiles.
        /// </summary>
        public Size Dimensions { get; }

        /// <summary>
        /// The size of the overview map, in terrain tiles.
        /// </summary>
        public Size OverviewDimensions => this.Dimensions * this.OverviewScale;

        /// <summary>
        /// The relative scale of the overview map in relation to the detailed map
        /// </summary>
        public float OverviewScale { get; }
        
        /// <summary>
        /// The seed used to generate the current map
        /// </summary>
        public int Seed { get; }
        
        /// <summary>
        /// The map the game is played on, full resolution.
        /// </summary>
        public TerrainType[,] DetailMap { get; } 
        
        /// <summary>
        /// The map used as an overview for orientation purposes. Each tile is the "average" terrain type
        /// based on 5x5 block in the detail map.
        /// </summary>
        public TerrainType[,] OverviewMap { get; }
        
        /// <summary>
        /// Rendering of the detailed map
        /// </summary>
        public Tile[,] DetailMapTiles { get; }
        
        /// <summary>
        /// Rendering of the overview map
        /// </summary>
        public Tile[,] OverviewMapTiles { get; }
        
        /// <summary>
        /// Rendering of the temperature map, for debugging purposes
        /// </summary>
        public Tile[,] TemperatureMapTiles { get; }
        
        /// <summary>
        /// Rendering of the rainfall map, for debugging purposes
        /// </summary>
        public Tile[,] RainfallMapTiles { get; }
        
        /// <summary>
        /// Overview rendering of the rainfall map
        /// </summary>
        public Tile[,] RainfallOverviewTiles { get; }
        
        /// <summary>
        /// Overview rendering of the temperature
        /// </summary>
        public Tile[,] TemperatureOverviewTiles { get; }
        
        /// <summary>
        /// Internal constructor
        /// </summary>
        private World(Size dimensions, int seed, float overviewScale = 0.05f)
        {
            this.Seed = seed;
            this.OverviewScale = overviewScale;
            this.Dimensions = dimensions;
            this.DetailMap = new TerrainType[this.Dimensions.Width, this.Dimensions.Height];
            this.DetailMapTiles = new Tile[this.Dimensions.Width, this.Dimensions.Height];
            this.OverviewMap = new TerrainType[this.OverviewDimensions.Width, this.OverviewDimensions.Height];
            this.OverviewMapTiles = new Tile[this.OverviewDimensions.Width, this.OverviewDimensions.Height];

            this.TemperatureMapTiles = new Tile[this.Dimensions.Width, this.Dimensions.Height];
            this.RainfallMapTiles = new Tile[this.Dimensions.Width, this.Dimensions.Height];
            
            this.RainfallOverviewTiles = new Tile[this.OverviewDimensions.Width, this.OverviewDimensions.Height];
            this.TemperatureOverviewTiles = new Tile[this.OverviewDimensions.Width, this.OverviewDimensions.Height];
        }

        /// <summary>
        /// Retrieves the terrain type at given position
        /// </summary>
        public TerrainType GetTerrainType(Position position)
        {
            return this.DetailMap[position.X, position.Y];
        }

        /// <summary>
        /// Retrieves the terrain type info at given position
        /// </summary>
        public TerrainTypeInfo GetTerrainInfo(Position position)
        {
            return TerrainTypeData.GetInfo(this.GetTerrainType(position));
        }

        /// <summary>
        /// Update the stored tiles to be up-to-date with the current terrain type map
        /// </summary>
        public void UpdateTiles()
        {
            this.UpdateDetailed();
            this.UpdateOverview();
        }

        /// <summary>
        /// Update tiles for detailed map
        /// </summary>
        private void UpdateDetailed()
        {
            var random = new Random(this.Seed);

            for (var ix = 0; ix < Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < Dimensions.Height; ++iy)
                {
                    var info = this.GetTerrainInfo(new Position(ix, iy));
                    var tile = (random.NextDouble() > 0.5) ? info.Primary : info.Secondary;
                    this.DetailMapTiles[ix, iy] = tile;
                }
            }
        }

        /// <summary>
        /// Update the overview map tiles and terrain types based on the current detailed map
        /// </summary>
        private void UpdateOverview()
        {
            var random = new Random(this.Seed);
            var terrainTypes = new List<TerrainType>();
            var scaleFactor = (int)(1.0f / this.OverviewScale);
            
            for (var ix = 0; ix < this.OverviewDimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.OverviewDimensions.Height; ++iy)
                {
                    var topLeft = new Position(ix * scaleFactor, iy * scaleFactor);
                    var bottomRight = new Position(topLeft.X + 4, topLeft.Y + 4);

                    terrainTypes.Clear();
                    
                    for (var iix = topLeft.X; iix <= bottomRight.X; ++iix)
                    {
                        for (var iiy = topLeft.Y; iiy <= bottomRight.Y; ++iiy)
                        {
                            terrainTypes.Add(this.DetailMap[iix, iiy]);
                        }
                    }
                    
                    var average = terrainTypes
                        .GroupBy(x => x)
                        .Select(x => new
                        {
                            Count = x.Count(),
                            Type = x.Key
                        })
                        .OrderByDescending(x => x.Count)
                        .Select(x => x.Type)
                        .First();

                    this.OverviewMap[ix, iy] = average;

                    var terrainInfo = TerrainTypeData.GetInfo(average);
                    this.OverviewMapTiles[ix, iy] =
                        (random.NextDouble() > 0.5) ? terrainInfo.Primary : terrainInfo.Secondary;
                }
            }
        }
    }
}