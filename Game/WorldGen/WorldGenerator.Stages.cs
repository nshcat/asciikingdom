using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using Engine.Core;
using Engine.Graphics;
using Game.Data;
using Game.Maths;
using Game.Simulation;
using Game.Utility;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;
using Range = Game.Maths.FloatRange;

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

            this.SignalNextStage("Generating drainage map..", 0.30);
            var drainageMap = new DrainageMap(this.WorldDimensions, this.Seed, this.Parameters, heightMap);

            this.SignalNextStage("Generating rainfall map..", 0.45);
            var rainfallMap = new RainfallMap(this.WorldDimensions, this.Seed, this.Parameters, heightMap);

            // Generate rivers, readjust rainfall map and rebuild levels
            this.SignalNextStage("Generating biomes..", 0.60);
            var biomeMapper = new BiomeMapper(this.WorldDimensions, this.Seed, heightMap, rainfallMap, drainageMap, temperatureMap);

            this.SignalNextStage("Generating rivers..", 0.70);
            var riverGenerator = new RiverGenerator(this.WorldDimensions, this.Seed, heightMap, temperatureMap, rainfallMap, biomeMapper.TerrainTypes);
            riverGenerator.GenerateRivers();

            this.SignalNextStage("Placing resources..", 0.75);
            var resourceGenerator = new ResourceGenerator(this.WorldDimensions, this.Seed, biomeMapper.TerrainTypes, this.Parameters);

            this.SignalNextStage("Storing data..", 0.80);
            world.DetailedMap.TerrainLayer = new TerrainWorldLayer(world.Dimensions, "terrain", "Terrain", dontAllocate: true);
            world.DetailedMap.TerrainLayer.Values = biomeMapper.TerrainTypes;

            this.AddLayer(world, this.CreateTileLayer("temperature", "Temperature", temperatureMap.TemperatureTiles));
            this.AddLayer(world, this.CreateTileLayer("drainage", "Drainage", drainageMap.DrainageTiles));
            this.AddLayer(world, this.CreateTileLayer("rainfall", "Rainfall", rainfallMap.RainfallTiles));

            this.AddLayer(world, this.CreateRawLayer("raw_rainfall", "Rainfall (raw)", rainfallMap.Values));
            this.AddLayer(world, this.CreateRawLayer("raw_drainage", "Drainage (raw)", drainageMap.Values));
            this.AddLayer(world, this.CreateRawLayer("raw_temperature", "Temperature (raw)", temperatureMap.Values));
            world.DetailedMap.RiverTileInfo = riverGenerator.RiverTileInfo;

            this.SignalNextStage("Creating terrain tiles..", 0.85);
            this.CreateTerrainTiles(world);

            this.SignalNextStage("Finding start continent..", 0.90);
            world.DiscoverInitialContinent();

            this.SignalNextStage("Building overview map..", 1.0);
            this.BuildOverview(world);

            // Signal that world generation has finished
            this.SignalFinished(world);
        }

        /// <summary>
        /// Add layer to detailed map of given world
        /// </summary>
        private void AddLayer(World world, WorldLayer layer)
        {
            world.DetailedMap.Layers.Add(layer.Id, layer);
        }

        /// <summary>
        /// Create map layer based on given raw float data
        /// </summary>
        private WorldLayer CreateRawLayer(string id, string name, float[,] data)
        {
            var layer = new RawWorldLayer(new Size(data.GetLength(0), data.GetLength(1)), id, name, dontAllocate: true);
            layer.Values = data;
            return layer;
        }

        /// <summary>
        /// Create map layer based on given tile data
        /// </summary>
        private WorldLayer CreateTileLayer(string id, string name, Tile[,] data)
        {
            var layer = new TileWorldLayer(new Size(data.GetLength(0), data.GetLength(1)), id, name, dontAllocate: true);
            layer.Values = data;
            return layer;
        }

        /// <summary>
        /// Determine detailed terrain tiles based on terrain types and rivers
        /// </summary>
        /// <param name="world"></param>
        private void CreateTerrainTiles(World world)
        {
            world.DetailedMap.TerrainTileLayer =
                TerrainTileLayerGenerator.CreateTileLayer(this.Seed, world.DetailedMap.TerrainLayer, world.DetailedMap.RiverTileInfo);
        }

        /// <summary>
        /// Initialize overview map from detailed map
        /// </summary>
        /// <param name="world"></param>
        private void BuildOverview(World world)
        {
            world.OverviewMap.InitializeFromDetailed(world.DetailedMap, world.OverviewScale);
        }

        /// <summary>
        /// Determine biomes and terrain features
        /// </summary>
        private void GenerateTerrain(World world, HeightMap heightMap, TemperatureMap temperatureMap)
        {
            for (var ix = 0; ix < this.WorldDimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.WorldDimensions.Height; ++iy)
                {
                    var height = heightMap.HeightLevels[ix, iy];
                    var temperature = temperatureMap.TemperatureLevels[ix, iy];
                    var terrainType = this.DetermineTerrain(height);

                    world.DetailedMap.TerrainLayer.Values[ix, iy] = terrainType;
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