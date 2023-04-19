using System;
using System.Collections.Generic;
using Engine.Core;
using Game.Core;
using Game.Data;
using Game.Maths;

namespace Game.WorldGen
{
    /// <summary>
    /// Class responsible for placing special resources on the world map
    /// </summary>
    public class ResourceGenerator
    {
        /// <summary>
        /// The world height map
        /// </summary>
        protected float[,] HeightMap { get; }

        /// <summary>
        /// The world temperature map
        /// </summary>
        protected float[,] TemperatureMap { get; }

        /// <summary>
        /// The world drainage map
        /// </summary>
        protected float[,] DrainageMap { get; }

        /// <summary>
        /// The map generator seed
        /// </summary>
        protected int Seed { get; }
        
        /// <summary>
        /// World dimensions
        /// </summary>
        protected Size Dimensions { get; }
        
        /// <summary>
        /// World generator parameters
        /// </summary>
        protected WorldParameters Parameters { get; }

        /// <summary>
        /// The calculated resource abundances
        /// </summary>
        public Dictionary<string, float[,]> ResourceAbundances { get; }
            = new Dictionary<string, float[,]>();
       
        /// <summary>
        /// Construct new resource generator instance
        /// </summary>
        public ResourceGenerator(   Size dimensions, int seed,
                                    float[,] heightmap,
                                    float[,] temperature,
                                    float[,] drainage,
                                    WorldParameters parameters)
        {
            HeightMap = heightmap;
            TemperatureMap = temperature;
            DrainageMap = drainage;
            Seed = seed;
            Dimensions = dimensions;
            Parameters = parameters;

            this.GenerateResources();
        }

        /// <summary>
        /// Generate special resources on the world map
        /// </summary>
        protected void GenerateResources()
        {
            
        }
    }
}