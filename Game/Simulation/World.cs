using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Core;
using Game.Data;

namespace Game.Simulation
{
    /// <summary>
    /// Represents the game world.
    /// </summary>
    public class World
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
        /// The detailed play map
        /// </summary>
        public DetailedMap DetailedMap { get; }
        
        /// <summary>
        /// The overview map
        /// </summary>
        public Map OverviewMap { get; }
        
        /// <summary>
        /// Internal constructor
        /// </summary>
        public World(Size dimensions, int seed, float overviewScale = 0.1250f)
        {
            this.Seed = seed;
            this.OverviewScale = overviewScale;
            this.Dimensions = dimensions;
            
            this.DetailedMap = new DetailedMap(dimensions, seed);
            this.OverviewMap = new Map(this.OverviewDimensions, seed);
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
    }
}