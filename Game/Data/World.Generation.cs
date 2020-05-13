// using System;
// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;
// using System.Linq;
// using System.Security.Cryptography;
// using System.Security.Cryptography.X509Certificates;
// using System.Xml.Schema;
// using Engine.Core;
// using Engine.Graphics;
// using Game.Maths;
// using OpenToolkit.Graphics.OpenGL;
// using SharpNoise;
// using SharpNoise.Builders;
// using SharpNoise.Models;
// using SharpNoise.Models;
// using SharpNoise.Modules;
// using Range = Game.Maths.Range;
// using Size = Engine.Core.Size;
// using TinkerWorX.AccidentalNoiseLibrary;
//
// namespace Game.Data
// {
//     public partial class WorldOld
//     {
//         /// <summary>
//         /// Represents the different temperature levels that can appear
//         /// on the world map
//         /// </summary>
//         private enum TemperatureLevel
//         {
//             Coldest,
//             Colder,
//             Cold,
//             Warm,
//             Warmer,
//             Warmest
//         }
//
//         /// <summary>
//         /// Represents the different moisture levels that can appear on
//         /// the world map
//         /// </summary>
//         private enum MoistureLevel
//         {
//             Wettest,
//             Wetter,
//             Wet,
//             Dry,
//             Dryer,
//             Dryest
//         }
//
//         /// <summary>
//         /// Generate a new world of given dimensions
//         /// </summary>
//         public static World GenerateWorld(Size dimensions, int seed)
//         {
//             var world = new World(dimensions, seed);
//
//             //var bounds = new System.Drawing.RectangleF(6, 1, 4, 4); default
//             //var bounds = new System.Drawing.RectangleF(6, 1, 6, 6); very nice
//             
//
//             var heightMap = GenerateHeightmap(dimensions, seed, bounds);
//             var tempMap = GenerateTemperatureMap(dimensions, heightMap, seed);
//             
//             GenerateDetailed(world, heightMap, tempMap, /*rainMap,*/ seed);
//
//             GenerateAltOverview(world, tempMap /*, rainMap*/);
//             
//             world.UpdateTiles();
//             
//             return world;
//         }
//
//         /// <summary>
//         /// Generate the overview versions of the temperature and rainfall maps
//         /// </summary>
//         private static void GenerateAltOverview(World world, TemperatureLevel[,] tempMap)
//         {
//             var scaleFactor = (int)(1.0f / world.OverviewScale);
//
//             var temperatures = new List<TemperatureLevel>();
//             
//             for (var ix = 0; ix < world.OverviewDimensions.Width; ++ix)
//             {
//                 for (var iy = 0; iy < world.OverviewDimensions.Height; ++iy)
//                 {
//                     var topLeft = new Position(ix * scaleFactor, iy * scaleFactor);
//                     var bottomRight = new Position(topLeft.X + 4, topLeft.Y + 4);
//
//                     temperatures.Clear();
//                     
//                     for (var iix = topLeft.X; iix <= bottomRight.X; ++iix)
//                     {
//                         for (var iiy = topLeft.Y; iiy <= bottomRight.Y; ++iiy)
//                         {
//                             temperatures.Add(tempMap[iix, iiy]);
//                         }
//                     }
//                     
//                     var average = temperatures
//                         .GroupBy(x => x)
//                         .Select(x => new
//                         {
//                             Count = x.Count(),
//                             Type = x.Key
//                         })
//                         .OrderByDescending(x => x.Count)
//                         .Select(x => x.Type)
//                         .First();
//
//                     world.TemperatureOverviewTiles[ix, iy] = new Tile(0, DefaultColors.Black, GetTemperatureColor(average));
//                 }
//             }
//         }
//
//         /// <summary>
//         /// Generate the temperature map
//         /// </summary>
//         private static TemperatureLevel[,] GenerateTemperatureMap(Size dimensions, NoiseMap heightMap, int seed)
//         {
//             var gradient = new ImplicitGradient(1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1);
//
//             var heatFractal = new ImplicitFractal(FractalType.Multi,
//                 BasisType.Simplex,
//                 InterpolationType.Quintic)
//             {
//                 Frequency = 3.0,
//                 Octaves = 4,
//                 Seed = seed
//             };
//
//             var heatMap = new ImplicitCombiner(CombinerType.Multiply);
//             heatMap.AddSource(gradient);
//             heatMap.AddSource(heatFractal);
//
//             // Noise range
//             double x1 = 0, x2 = 2;
//             double y1 = 0, y2 = 2;
//             var dx = x2 - x1;
//             var dy = y2 - y1;
//
//             var temperatures = new float[dimensions.Width, dimensions.Height];
//             var min = float.MaxValue;
//             var max = float.MinValue;
//
//             for (var ix = 0; ix < dimensions.Width; ++ix)
//             {
//                 for (var iy = 0; iy < dimensions.Height; ++iy)
//                 {
//                     double u = ix / (float) dimensions.Width;
//                     double v = iy / (float) dimensions.Height;
//
//                     /*var nx = (float) (x1 + (u * dx));
//                     var ny = (float) (y1 + (v * dy));
//                     var temperatureValue = (float) heatMap.Get(nx, ny);*/
//                     
//                     var nx = (float) (x1 + Math.Cos(u * 2 * Math.PI) * dx / (2 * Math.PI));
//                     var ny = (float) (y1 + Math.Cos(v * 2 * Math.PI) * dy / (2 * Math.PI));
//                     var nz = (float) (x1 + Math.Sin(u * 2 * Math.PI) * dx / (2 * Math.PI));
//                     var nw = (float) (y1 + Math.Sin(v * 2 * Math.PI) * dy / (2 * Math.PI));
//
//                     var temperatureValue = (float) heatMap.Get(nx, ny, nz, nw);
//
//                     temperatures[ix, iy] = temperatureValue;
//
//                     if (temperatureValue < min) min = temperatureValue;
//                     if (temperatureValue > max) max = temperatureValue;
//                 }
//             }
//
//             var sourceRange = new Range(min, max);
//             var destRange = new Range(0.0f, 1.0f);
//             var heightSourceRange = new Range(0.4f, 1.0f);
//             var levels = new TemperatureLevel[dimensions.Width, dimensions.Height];
//
//             for (var ix = 0; ix < dimensions.Width; ++ix)
//             {
//                 for (var iy = 0; iy < dimensions.Height; ++iy)
//                 {
//                     var value = MathUtil.Map(temperatures[ix, iy], sourceRange, destRange);
//                     
//                     // Factor in height
//                     var height = heightMap[ix, iy];
//
//                     if (height > 0.4f)
//                     {
//                         var scaled = MathUtil.Map(height, heightSourceRange, destRange);
//
//                         value = Math.Clamp(value - (scaled * 0.35f /** 0.45f*//** 0.3f*/), 0.0f, 1.0f);
//                     }
//                     
//                     var level = TemperatureLevel.Warmest;
//
//                     if (value < 0.04f)
//                         level = TemperatureLevel.Coldest;
//                     else if (value < 0.15f)
//                         level = TemperatureLevel.Colder;
//                     else if (value < 0.4f)
//                         level = TemperatureLevel.Cold;
//                     else if (value < 0.6f)
//                         level = TemperatureLevel.Warm;
//                     else if (value < 0.8f)
//                         level = TemperatureLevel.Warmer;
//                     
//                     /* Original values
//                       if (value < 0.05f)
//                         level = TemperatureLevel.Coldest;
//                     else if (value < 0.18f)
//                         level = TemperatureLevel.Colder;
//                     else if (value < 0.4f)
//                         level = TemperatureLevel.Cold;
//                     else if (value < 0.6f)
//                         level = TemperatureLevel.Warm;
//                     else if (value < 0.8f)
//                         level = TemperatureLevel.Warmer;*/
//
//                     levels[ix, iy] = level;
//                 }
//             }
//
//             return levels;
//         }
//
//         /// <summary>
//         /// Generate the terrain height map
//         /// </summary>
//         private static NoiseMap GenerateHeightmap(Size dimensions, int seed, System.Drawing.RectangleF bounds)
//         {
//             var noiseModule = BuildModuleTree(seed);
//             return GenerateNoisemap(dimensions, noiseModule, bounds);
//         }
//
//         /// <summary>
//         /// Uses given noise module tree to generate a noise map
//         /// </summary>
//         private static NoiseMap GenerateNoisemap(Size dimensions, Module rootModule, System.Drawing.RectangleF bounds)
//         {
//             var noiseMap = new NoiseMap();
//             var builder = new PlaneNoiseMapBuilder()
//             {
//                 DestNoiseMap = noiseMap,
//                 SourceModule = rootModule
//             };
//             builder.SetDestSize(dimensions.Width, dimensions.Height);
//             
//             builder.SetBounds(bounds.Left, bounds.Right, bounds.Top, bounds.Bottom);
//             builder.Build();
//             
//             Normalize(noiseMap);
//
//             return noiseMap;
//         }
//
//         /// <summary>
//         /// Generate detailed map from given noise map
//         /// </summary>
//         private static void GenerateDetailed(World world, NoiseMap noiseMap, TemperatureLevel[,] temperatureMap/*, NoiseMap rainfallMap*/, int seed)
//         {
//             var random = new Random(seed);
//
//             for (var ix = 0; ix < noiseMap.Width; ++ix)
//             {
//                 for (var iy = 0; iy < noiseMap.Height; ++iy)
//                 {
//                     var height = noiseMap[ix, iy];
//                     var temperature = temperatureMap[ix, iy];
//                     //var rainfall = rainfallMap[ix, iy];
//                     var terrainType = DetermineTerrain(height);
//
//                     var temperatureTile = new Tile(0, DefaultColors.Black, GetTemperatureColor(temperature));
//                     //var rainTile = new Tile(0, DefaultColors.Black, Color.FromGrayscale(rainfall));
//                     
//                     world.DetailMap[ix, iy] = terrainType;
//                     world.RainfallMapTiles[ix, iy] = new Tile(0);
//                     world.TemperatureMapTiles[ix, iy] = temperatureTile;
//                 }
//             }
//         }
//
//         /// <summary>
//         /// Retrieve color for given temperature levels
//         /// </summary>
//         private static Color GetTemperatureColor(TemperatureLevel level)
//         {
//             switch (level)
//             {
//                 case TemperatureLevel.Coldest:
//                     return new Color(0, 255, 255);
//                 case TemperatureLevel.Colder:
//                     return new Color(170, 255, 255);
//                 case TemperatureLevel.Cold:
//                     return new Color(0, 229, 133);
//                 case TemperatureLevel.Warm:
//                     return new Color(255, 255, 100);
//                 case TemperatureLevel.Warmer:
//                     return new Color(255, 100, 0);
//                 default:
//                     return new Color(241, 12, 0);
//             }
//         }
//         
//         /// <summary>
//         /// Normalize the values contained within given noise map to be inside the range [0, 1].
//         /// </summary>
//         private static void Normalize(NoiseMap noiseMap)
//         {
//             // Determine range of values in noise map
//             var inputRange = new Maths.Range(noiseMap.Data.Min(), noiseMap.Data.Max());
//             var outputRange = new Maths.Range(0.0f, 1.0f);
//
//             for (var i = 0; i < noiseMap.Data.Length; ++i)
//             {
//                 var oldValue = noiseMap.Data[i];
//                 noiseMap.Data[i] = MathUtil.Map(oldValue, inputRange, outputRange);
//             }
//         }
//         
//         /// <summary>
//         /// Determine terrain type given the map height, temperature and rain
//         /// </summary>
//         private static TerrainType DetermineTerrain(float height/*, float temperature, float rain*/)
//         {
//             if (height <= 0.40f)
//                 return TerrainType.Ocean;
//             else if (height <= 0.7f)
//                 return TerrainType.Grassland;
//             else if (height <= 0.85)
//                 return TerrainType.MountainsLow;
//             else if (height <= 0.88)
//                 return TerrainType.MountainsMed;
//             else if (height <= 0.93)
//                 return TerrainType.MountainsHigh;
//             else
//                 return TerrainType.MountainPeak;
//
//             /*if (height <= 0.45f) // Generally ocean, but maybe glacier
//             {
//                 if (temperature <= 0.04f)
//                     return TerrainType.Glacier;
//                 else if (temperature <= 0.06f)
//                     return TerrainType.SeaIce;
//                 else
//                     return TerrainType.Ocean;
//             }
//             else
//             {    
//                 // In very cold places, even land gets replaced with glacier
//                 if (temperature < 0.03f)
//                     return TerrainType.Glacier;
//
//                 if (temperature < 0.2f)
//                     return TerrainType.Tundra;
//
//                 if (InRange(temperature, 0.2f, 0.6f) && InRange(rain, 0.0f, 0.2f))
//                     return TerrainType.GrasslandDry;
//
//                 if (InRange(temperature, 0.6f, 0.7f) && InRange(rain, 0.0f, 0.2f))
//                     return TerrainType.ShrublandDry;
//
//                 if (InRange(temperature, 0.7f, 0.8f) && InRange(rain, 0.0f, 0.2f))
//                     return TerrainType.RockyWasteland;
//
//                 if (InRange(temperature, 0.8f, 1.0f) && InRange(rain, 0.0f, 0.2f))
//                     return TerrainType.SandDesert;
//                 
//                 if (InRange(temperature, 0.2f, 0.4f) && InRange(rain, 0.2f, 0.4f))
//                     return TerrainType.Grassland;
//                 
//                 if (InRange(temperature, 0.4f, 0.6f) && InRange(rain, 0.2f, 0.5f))
//                     return TerrainType.Grassland;
//
//                 if (InRange(temperature, 0.6f, 0.8f) && InRange(rain, 0.2f, 0.3f))
//                     return TerrainType.Shrubland;
//
//                 if (InRange(temperature, 0.8f, 1.0f) && InRange(rain, 0.2f, 0.3f))
//                     return TerrainType.RockyWasteland;
//
//                 if (InRange(temperature, 0.6f, 0.8f) && InRange(rain, 0.3f, 0.4f))
//                     return TerrainType.GrasslandDry;
//
//                 if (InRange(temperature, 0.8f, 1.0f) && InRange(rain, 0.3f, 0.4f))
//                     return TerrainType.SavannaDry;
//
//                 if (InRange(temperature, 0.6f, 0.8f) && InRange(rain, 0.4f, 0.5f))
//                     return TerrainType.Grassland;
//
//                 if (InRange(temperature, 0.6f, 0.8f) && InRange(rain, 0.5f, 0.9f))
//                     return TerrainType.TemperateBroadleafForest;
//                 
//                 if (InRange(temperature, 0.4f, 0.6f) && InRange(rain, 0.5f, 0.9f))
//                     return TerrainType.TemperateBroadleafForest;
//
//                 if (InRange(temperature, 0.8f, 1.0f) && InRange(rain, 0.4f, 0.7f))
//                     return TerrainType.Savanna;
//
//                 if (InRange(temperature, 0.8f, 1.0f) && InRange(rain, 0.7f, 1.0f))
//                     return TerrainType.TropicalBroadleafForest;
//
//                 if (InRange(temperature, 0.6f, 0.8f) && InRange(rain, 0.9f, 1.0f))
//                     return TerrainType.Swamp;
//                 
//                 if (InRange(temperature, 0.2f, 0.6f) && InRange(rain, 0.9f, 1.0f))
//                     return TerrainType.Marsh;
//
//                 if (InRange(temperature, 0.2f, 0.4f) && InRange(rain, 0.4f, 0.9f))
//                     return TerrainType.ConiferousForest;
//
//                 return TerrainType.Unknown;
//             }*/
//         }
//
//         /// <summary>
//         /// Check whether value lies inside the given range.
//         /// </summary>
//         private static bool InRange(float x, float min, float max)
//         {
//             return x >= min && x <= max;
//         }
//     }
// }