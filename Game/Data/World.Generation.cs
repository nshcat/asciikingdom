using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Schema;
using Engine.Core;
using Engine.Graphics;
using Game.Maths;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Models;
using SharpNoise.Models;
using SharpNoise.Modules;
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

            var noiseModule = BuildModuleTree(seed);
            var noiseMap = new NoiseMap();
            var builder = new PlaneNoiseMapBuilder()
            {
                DestNoiseMap = noiseMap,
                SourceModule = noiseModule
            };
            builder.SetDestSize(dimensions.Width, dimensions.Height);
            
            //var bounds = new System.Drawing.RectangleF(6, 1, 4, 4);
            var bounds = new System.Drawing.RectangleF(6, 1, 6, 6);
            builder.SetBounds(bounds.Left, bounds.Right, bounds.Top, bounds.Bottom);
            builder.Build();
            
            Normalize(noiseMap);

            GenerateDetailed(world, noiseMap, seed);

            GenerateOverview(world, seed);
            
            return world;
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
                    world.DetailMapTiles[ix, iy] = tile;
                }
            }
        }
        

        /// <summary>
        /// Build the noise module tree
        /// </summary>
        private static Module BuildModuleTree(int seed)
        {
            var mountainTerrain = new RidgedMulti()
            {
                Seed = seed
            };

            var baseFlatTerrain = new Billow()
            {
                Seed = seed,
                Frequency = 2
            };

            var flatTerrain = new ScaleBias()
            {
                Source0 = baseFlatTerrain,
                Scale = 0.125,
                Bias = -0.75
            };

            var terrainType1 = new Perlin()
            {
                Frequency = 0.5,
                Persistence = 0.25,
                Seed = seed
            };
            
            var terrainType2 = new Perlin()
            {
                Frequency = 0.5,
                Persistence = 0.25,
                Seed = seed + 1337
            };

            var terrainType = new Multiply()
            {
                Source0 = terrainType1,
                Source1 = terrainType2
            };

            var terrainSelector = new Select()
            {
                Source0 = flatTerrain,
                Source1 = mountainTerrain,
                Control = terrainType,
                LowerBound = 0,
                UpperBound = 1000,
                EdgeFalloff = 0.125
            };

            var finalTerrain = new Turbulence()
            {
                Source0 = terrainSelector,
                Frequency = 4,
                Power = 0.125,
                Seed = seed
            };

            return finalTerrain;
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
        /// Generate the overview map for given world
        /// </summary>
        private static void GenerateOverview(World world, int seed)
        {
            var random = new Random(seed);
            var terrainTypes = new List<TerrainType>();
            
            for (var ix = 0; ix < world.OverviewDimensions.Width; ++ix)
            {
                for (var iy = 0; iy < world.OverviewDimensions.Height; ++iy)
                {
                    var topLeft = new Position(ix * 5, iy * 5);
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