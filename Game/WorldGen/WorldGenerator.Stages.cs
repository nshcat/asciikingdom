using System;
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
            
            this.SignalNextStage("Generating temperature map..", 0.15);
            var temperatureMap = this.GenerateTemperatureMap(heightMap);
            
            this.SignalNextStage("Generating terrain..", 0.40);
            this.GenerateTerrain(world, heightMap, temperatureMap);
            
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
        private void GenerateTerrain(World world, NoiseMap heightMap, TemperatureLevel[,] temperatureLevels)
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
        private TerrainType DetermineTerrain(float height /*, float temperature, float rain*/)
        {
            if (height <= 0.40f)
                return TerrainType.Ocean;
            else if (height <= 0.7f)
                return TerrainType.Grassland;
            else if (height <= 0.85)
                return TerrainType.MountainsLow;
            else if (height <= 0.88)
                return TerrainType.MountainsMed;
            else if (height <= 0.93)
                return TerrainType.MountainsHigh;
            else
                return TerrainType.MountainPeak;
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

                    if (value < 0.04f)
                        level = TemperatureLevel.Coldest;
                    else if (value < 0.15f)
                        level = TemperatureLevel.Colder;
                    else if (value < 0.4f)
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