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
            var heightMap = new HeightMap(this.WorldDimensions, this.Seed, this.Parameters);

            this.SignalNextStage("Generating temperature map..", 0.25);
            var temperatureMap = new TemperatureMap(this.WorldDimensions, this.Seed, this.Parameters, heightMap);
            world.DetailedMap.Temperature = temperatureMap.TemperatureTiles;
            
            this.SignalNextStage("Generating terrain..", 0.40);
            this.GenerateTerrain(world, heightMap.HeightLevels, temperatureMap.TemperatureLevels);

            this.SignalNextStage("Updating terrain tiles..", 0.75);
            world.UpdateTiles();
            
            this.SignalNextStage("Building overview map..", 0.90);
            world.BuildOverview();

            // Signal that world generation has finished
            this.SignalFinished(world);
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
    }
}