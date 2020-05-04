using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Schema;
using Engine.Core;
using Engine.Graphics;
using Game.Maths;
using OpenToolkit.Graphics.OpenGL;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Models;
using SharpNoise.Models;
using SharpNoise.Modules;
using Range = Game.Maths.Range;
using Size = Engine.Core.Size;

namespace Game.Data
{
    public partial class World
    {
        /// <summary>
        /// Generate a new world of given dimensions
        /// </summary>
        public static World GenerateWorld(Size dimensions, int seed)
        {
            var world = new World(dimensions);

            //var bounds = new System.Drawing.RectangleF(6, 1, 4, 4); default
            //var bounds = new System.Drawing.RectangleF(6, 1, 6, 6); very nice
            var bounds = new System.Drawing.RectangleF(6, 1, 3, 3);

            var heightMap = GenerateHeightmap(dimensions, seed, bounds);
            var tempMap = GenerateTemperatureMap(dimensions, seed + 512, bounds);
            var rainMap = GenerateRainfallMap(dimensions);
            
            GenerateDetailed(world, rainMap, seed);

            GenerateOverview(world, seed);
            
            return world;
        }

        /// <summary>
        /// Generate the terrain height map
        /// </summary>
        private static NoiseMap GenerateHeightmap(Size dimensions, int seed, System.Drawing.RectangleF bounds)
        {
            var noiseModule = BuildModuleTree(seed);
            return GenerateNoisemap(dimensions, noiseModule, bounds);
        }
        
        /// <summary>
        /// Generate the temperature map
        /// </summary>
        private static NoiseMap GenerateTemperatureMap(Size dimensions, int seed, System.Drawing.RectangleF bounds)
        {
            var noiseModule = BuildTemperatureModuleTree(seed);
            var tempMap = GenerateNoisemap(dimensions, noiseModule, bounds);
            
            // Apply general temperature gradient (makes north and south pole very cold)
            for (var iy = 0; iy < dimensions.Height; ++iy)
            {
                var x = MathUtil.Map(iy, new Range(0.0f, dimensions.Height - 1), new Range(0.0f, 1.0f));
                
                // Black magic
                var factor = (float)Math.Pow((-0.43 * Math.Pow(3.0*x - 1.5, 2.0) + 1.0), 1/1.3);

                for (var ix = 0; ix < dimensions.Width; ++ix)
                {
                    tempMap[ix, iy] *= factor;
                }
            }

            return tempMap;
        }

        /// <summary>
        /// Uses given noise module tree to generate a noise map
        /// </summary>
        private static NoiseMap GenerateNoisemap(Size dimensions, Module rootModule, System.Drawing.RectangleF bounds)
        {
            var noiseMap = new NoiseMap();
            var builder = new PlaneNoiseMapBuilder()
            {
                DestNoiseMap = noiseMap,
                SourceModule = rootModule
            };
            builder.SetDestSize(dimensions.Width, dimensions.Height);
            
            builder.SetBounds(bounds.Left, bounds.Right, bounds.Top, bounds.Bottom);
            builder.Build();
            
            Normalize(noiseMap);

            return noiseMap;
        }

        /// <summary>
        /// Generate detailed map from given noise map
        /// </summary>
        private static void GenerateDetailed(World world, NoiseMap noiseMap, int seed)
        {
            var random = new Random(seed);

            for (var ix = 0; ix < noiseMap.Width; ++ix)
            {
                for (var iy = 0; iy < noiseMap.Height; ++iy)
                {
                    var height = noiseMap[ix, iy];
                    var terrainType = DetermineTerrain(height, 0.0f, 0.0f);
                    var terrainInfo = TerrainTypeData.GetInfo(terrainType);
                    var tile = (random.NextDouble() > 0.5) ? terrainInfo.Primary : terrainInfo.Secondary;

                    var color = (int) (height * 255);
                    var grayScale = new Color(color, color, color);
                    var tile2 = new Tile(0, DefaultColors.Black, grayScale);
                    
                    world.DetailMap[ix, iy] = terrainType;
                    world.DetailMapTiles[ix, iy] = tile2;
                }
            }
        }
        
        /// <summary>
        /// Normalize the values contained within given noise map to be inside the range [0, 1].
        /// </summary>
        private static void Normalize(NoiseMap noiseMap)
        {
            // Determine range of values in noise map
            var inputRange = new Maths.Range(noiseMap.Data.Min(), noiseMap.Data.Max());
            var outputRange = new Maths.Range(0.0f, 1.0f);

            for (var i = 0; i < noiseMap.Data.Length; ++i)
            {
                var oldValue = noiseMap.Data[i];
                noiseMap.Data[i] = MathUtil.Map(oldValue, inputRange, outputRange);
            }
        }
        
        /// <summary>
        /// Determine terrain type given the map height, temperature and rain
        /// </summary>
        private static TerrainType DetermineTerrain(float height, float temperature, float rain)
        {
            if (height <= 0.45f)
                return TerrainType.Ocean;
            else if (height <= 0.70)
                return TerrainType.Grassland;
            else if (height <= 0.80)
                return TerrainType.Hills;
            else if (height <= 0.85f)
                return TerrainType.MountainsLow;
            else if (height <= 0.90f)
                return TerrainType.MountainsMed;
            else if (height <= 0.95f)
                return TerrainType.MountainsHigh;
            else
                return TerrainType.MountainPeak;
        }

        /// <summary>
        /// Generates the rainfall map
        /// </summary>
        private static NoiseMap GenerateRainfallMap(Size dimensions)
        {
            var map = new NoiseMap(dimensions.Width, dimensions.Height);

            for (var iy = 0; iy < dimensions.Height; ++iy)
            {
                var y = MathUtil.Map(iy, new Range(0.0f, dimensions.Height - 1), new Range(0.0f, 1.0f));

                var rainfall =
                    0.03023253
                    + 8.248102 * y
                    - 42.08078 * Math.Pow(y, 2.0)
                    + 73.53609 * Math.Pow(y, 3.0)
                    - 42.44075 * Math.Pow(y, 4.0)
                    + 2.850729 * Math.Pow(y, 5.0);

                for (var ix = 0; ix < dimensions.Width; ++ix)
                {
                    map[ix, iy] = (float)Math.Clamp(rainfall, 0.0, 1.0);
                }
            }

            return map;
        }

        /// <summary>
        /// Generate the overview map for given world
        /// </summary>
        private static void GenerateOverview(World world, int seed)
        {
            var random = new Random(seed);
            var terrainTypes = new List<TerrainType>();
            var scaleFactor = (int)(1.0f / world.OverviewScale);
            
            for (var ix = 0; ix < world.OverviewDimensions.Width; ++ix)
            {
                for (var iy = 0; iy < world.OverviewDimensions.Height; ++iy)
                {
                    var topLeft = new Position(ix * scaleFactor, iy * scaleFactor);
                    var bottomRight = new Position(topLeft.X + 4, topLeft.Y + 4);

                    terrainTypes.Clear();
                    
                    for (var iix = topLeft.X; iix <= bottomRight.X; ++iix)
                    {
                        for (var iiy = topLeft.Y; iiy <= bottomRight.Y; ++iiy)
                        {
                            terrainTypes.Add(world.DetailMap[iix, iiy]);
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

                    world.OverviewMap[ix, iy] = average;

                    var terrainInfo = TerrainTypeData.GetInfo(average);
                    world.OverviewMapTiles[ix, iy] =
                        (random.NextDouble() > 0.5) ? terrainInfo.Primary : terrainInfo.Secondary;
                }
            }
        }
    }
}