using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using Engine.Core;
using Engine.Graphics;
using Game.Data;
using Game.Maths;
using Game.Simulation;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;
using TinkerWorX.AccidentalNoiseLibrary;
using Range = Game.Maths.Range;

namespace Game.WorldGen
{
    public partial class WorldGenerator
    {
        /// <summary>
        /// The actual world generation process
        /// </summary>
        protected override void DoOperation()
        {
            // Generate empty world object
            var world = new World(this.WorldDimensions, this.Seed);
            
            this.SignalNextStage("Generating height map..", 0.0);
            var heightMap = this.GenerateHeightmap();
            this.AccentuatePeaks(heightMap.Data);
            this.Normalize(heightMap);
            var heightLevels = this.DetermineHeightLevels(heightMap);

            this.SignalNextStage("Generating temperature map..", 0.25);
            var temperatureMap = this.GenerateTemperatureMap(heightMap);
            
            this.SignalNextStage("Generating terrain..", 0.40);
            this.GenerateTerrain(world, heightLevels, temperatureMap);
            
            this.SignalNextStage("Rendering temperature map..", 0.60);
            this.RenderTemperatureMap(world, temperatureMap);
            
            this.SignalNextStage("Updating terrain tiles..", 0.75);
            world.UpdateTiles();
            
            this.SignalNextStage("Building overview map..", 0.90);
            world.BuildOverview();

            // Signal that world generation has finished
            this.SignalFinished(world);
        }

        /// <summary>
        /// Generate the terrain height map
        /// </summary>
        private NoiseMap GenerateHeightmap()
        {
            var bounds = new System.Drawing.RectangleF(6, 1, 3, 3);
            var noiseModule = BuildModuleTree(this.Seed);
            return GenerateNoiseMap(this.WorldDimensions, noiseModule, bounds);
        }

        /// <summary>
        /// Determine biomes and terrain features
        /// </summary>
        private void GenerateTerrain(World world, HeightLevel[,] heightMap, TemperatureLevel[,] temperatureLevels)
        {
            for (var ix = 0; ix < this.WorldDimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.WorldDimensions.Height; ++iy)
                {
                    var height = heightMap[ix, iy];
                    var temperature = temperatureLevels[ix, iy];
                    var terrainType = this.DetermineTerrain(height);

                    world.DetailedMap.Terrain[ix, iy] = terrainType;
                }
            }
        }
        
        /// <summary>
        /// Determine the height levels based on the height map with values in [0, 1] and the
        /// sea and tree level thresholds derived from the percentages given in <see cref="Parameters"/>
        /// </summary>
        private HeightLevel[,] DetermineHeightLevels(NoiseMap heightMap)
        {
            // TODO: For drainage, we need the final elevation values in [0, 1]. Therefore,
            // this function should be renamed into RefineHeightmap and return both the height levels
            // as well as a 2D float array with remapped elevation values corresponding to the various
            // height levels. Do we need to actually remap them? Or is it enough if the drainage generator
            // knows the sea and low hill thresholds? That might be enough!
            
            var levels = new HeightLevel[this.WorldDimensions.Width, this.WorldDimensions.Height];
            
            var lowMountainPercent =
                this.Parameters.TreeLinePercentage + (1.0f - this.Parameters.TreeLinePercentage) / 2.0f;
            var medMountainPercent =
                lowMountainPercent + (1.0f - lowMountainPercent) / 1.8f;
            
            var highMountainPercent =
                medMountainPercent + (1.0f - medMountainPercent) / 1.006f;
            
            var seaThreshold = this.CalculateThreshold(heightMap, this.Parameters.UnderWaterPercentage);
            var treeLineThreshold = this.CalculateThreshold(heightMap, this.Parameters.TreeLinePercentage);
            var lowMountainThreshold = this.CalculateThreshold(heightMap, lowMountainPercent);
            var medMountainThreshold = this.CalculateThreshold(heightMap, medMountainPercent);
            var highMountainThreshold = this.CalculateThreshold(heightMap, highMountainPercent);
            
            for (var ix = 0; ix < this.WorldDimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.WorldDimensions.Height; ++iy)
                {
                    var heightValue = heightMap[ix, iy];
                    var heightLevel = HeightLevel.MountainPeak;

                    if (heightValue <= seaThreshold)
                        heightLevel = HeightLevel.Sea;
                    else if (heightValue <= treeLineThreshold)
                        heightLevel = HeightLevel.Land;
                    else if (heightValue <= lowMountainThreshold)
                        heightLevel = HeightLevel.LowMountain;
                    else if (heightValue <= medMountainThreshold)
                        heightLevel = HeightLevel.MediumMountain;
                    else if (heightValue <= highMountainThreshold)
                        heightLevel = HeightLevel.HighMountain;

                    levels[ix, iy] = heightLevel;
                }
            }

            return levels;
        }
        
        /// <summary>
        /// Render the temperature map to tiles.
        /// </summary>
        private void RenderTemperatureMap(World world, TemperatureLevel[,] temperatureLevels)
        {
            for (var ix = 0; ix < this.WorldDimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.WorldDimensions.Height; ++iy)
                {
                    var color = this.GetTemperatureColor(temperatureLevels[ix, iy]);
                    world.DetailedMap.Temperature[ix, iy] = new Tile(0, DefaultColors.Black, color); 
                }
            }
        }

        /// <summary>
        /// Determine terrain type given the map height, temperature and rain
        /// </summary>
        private TerrainType DetermineTerrain(HeightLevel height/*, MoistureLevel rain, TemperatureLevel temperature*/)
        {
            switch (height)
            {
                case HeightLevel.Sea:
                    return TerrainType.Ocean;
                case HeightLevel.Land:
                    return TerrainType.Grassland;
                case HeightLevel.LowMountain:
                    return TerrainType.MountainsLow;
                case HeightLevel.MediumMountain:
                    return TerrainType.MountainsMed;
                case HeightLevel.HighMountain:
                    return TerrainType.MountainsHigh;
                case HeightLevel.MountainPeak:
                    return TerrainType.MountainPeak;
                default:
                    return TerrainType.Unknown;
            }
        }

        /// <summary>
        /// Generate the temperature map
        /// </summary>
        private TemperatureLevel[,] GenerateTemperatureMap(NoiseMap heightMap)
        {
            var dimensions = this.WorldDimensions;
            var gradient = new ImplicitGradient(1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1);

            var heatFractal = new ImplicitFractal(FractalType.Multi,
                BasisType.Simplex,
                InterpolationType.Quintic)
            {
                Frequency = 3.0,
                Octaves = 4,
                Seed = this.Seed
            };

            var heatMap = new ImplicitCombiner(CombinerType.Multiply);
            heatMap.AddSource(gradient);
            heatMap.AddSource(heatFractal);

            // Noise range
            double x1 = 0, x2 = 2;
            double y1 = 0, y2 = 2;
            var dx = x2 - x1;
            var dy = y2 - y1;

            var temperatures = new float[dimensions.Width, dimensions.Height];
            var min = float.MaxValue;
            var max = float.MinValue;

            for (var ix = 0; ix < dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < dimensions.Height; ++iy)
                {
                    double u = ix / (float) dimensions.Width;
                    double v = iy / (float) dimensions.Height;

                    /*var nx = (float) (x1 + (u * dx));
                    var ny = (float) (y1 + (v * dy));
                    var temperatureValue = (float) heatMap.Get(nx, ny);*/
                    
                    var nx = (float) (x1 + Math.Cos(u * 2 * Math.PI) * dx / (2 * Math.PI));
                    var ny = (float) (y1 + Math.Cos(v * 2 * Math.PI) * dy / (2 * Math.PI));
                    var nz = (float) (x1 + Math.Sin(u * 2 * Math.PI) * dx / (2 * Math.PI));
                    var nw = (float) (y1 + Math.Sin(v * 2 * Math.PI) * dy / (2 * Math.PI));

                    var temperatureValue = (float) heatMap.Get(nx, ny, nz, nw);

                    temperatures[ix, iy] = temperatureValue;

                    if (temperatureValue < min) min = temperatureValue;
                    if (temperatureValue > max) max = temperatureValue;
                }
            }

            var sourceRange = new Range(min, max);
            var destRange = new Range(0.0f, 1.0f);
            var heightSourceRange = new Range(0.4f, 1.0f);
            var levels = new TemperatureLevel[dimensions.Width, dimensions.Height];

            for (var ix = 0; ix < dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < dimensions.Height; ++iy)
                {
                    var value = MathUtil.Map(temperatures[ix, iy], sourceRange, destRange);
                    
                    // Factor in height
                    var height = heightMap[ix, iy];

                    if (height > 0.4f)
                    {
                        var scaled = MathUtil.Map(height, heightSourceRange, destRange);

                        value = Math.Clamp(value - (scaled * 0.35f /** 0.45f*//** 0.3f*/), 0.0f, 1.0f);
                    }
                    
                    var level = TemperatureLevel.Warmest;

                    if (value < 0.025f)
                        level = TemperatureLevel.Coldest;
                    else if (value < 0.10f)
                        level = TemperatureLevel.Colder;
                    else if (value < 0.35f)
                        level = TemperatureLevel.Cold;
                    else if (value < 0.6f)
                        level = TemperatureLevel.Warm;
                    else if (value < 0.8f)
                        level = TemperatureLevel.Warmer;
                    
                    /* Original values
                      if (value < 0.05f)
                        level = TemperatureLevel.Coldest;
                    else if (value < 0.18f)
                        level = TemperatureLevel.Colder;
                    else if (value < 0.4f)
                        level = TemperatureLevel.Cold;
                    else if (value < 0.6f)
                        level = TemperatureLevel.Warm;
                    else if (value < 0.8f)
                        level = TemperatureLevel.Warmer;*/

                    levels[ix, iy] = level;
                }
            }

            return levels;
        }
    }
}