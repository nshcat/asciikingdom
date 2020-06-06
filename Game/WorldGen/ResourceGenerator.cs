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
        /// The biome map
        /// </summary>
        protected TerrainType[,] Biomes { get; }
        
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
        /// Generated resources
        /// </summary>
        public Dictionary<Position, ResourceType> Resources { get; }
            = new Dictionary<Position, ResourceType>();
        
        /// <summary>
        /// Resource spawn tables for each biome type
        /// </summary>
        protected Dictionary<TerrainType, WeightedCollection<ResourceType>> SpawnTables { get; }
            = new Dictionary<TerrainType, WeightedCollection<ResourceType>>();

        /// <summary>
        /// Construct new resource generator instance
        /// </summary>
        public ResourceGenerator(Size dimensions, int seed, TerrainType[,] biomes, WorldParameters parameters)
        {
            Biomes = biomes;
            Seed = seed;
            Dimensions = dimensions;
            Parameters = parameters;

            this.BuildSpawnTables();
            this.GenerateResources();
        }

        /// <summary>
        /// Create and populate spawn tables based on known resource types
        /// </summary>
        protected void BuildSpawnTables()
        {
            var resourceTypes = ResourceTypeManager.Instance.AllTypes;

            foreach (var (_, type) in resourceTypes)
            {
                foreach (var spawnBiome in type.AllowedTerrain)
                {
                    if(!this.SpawnTables.ContainsKey(spawnBiome))
                        this.SpawnTables[spawnBiome] = new WeightedCollection<ResourceType>();
                    
                    this.SpawnTables[spawnBiome].Add(type.Weight, type);
                }
            }
        }

        /// <summary>
        /// Check if there is at least one resource type that can spawn in given biome
        /// </summary>
        protected bool HasResources(TerrainType biome)
        {
            return this.SpawnTables.ContainsKey(biome);
        }

        /// <summary>
        /// Select a special resource type for a tile with given biome
        /// </summary>
        protected ResourceType SelectResource(Random rng, TerrainType biome)
        {
            var collection = this.SpawnTables[biome];
            return collection.Next(rng);
        }

        /// <summary>
        /// Generate special resources on the world map
        /// </summary>
        protected void GenerateResources()
        {
            var sampler = new PoissonDiskSampler(this.Dimensions, 15.0f);
            var rng = new Random(this.Seed + 9118);
            var positions = sampler.Sample(rng);

            foreach (var position in positions)
            {
                var biome = this.Biomes[position.X, position.Y];

                if (this.HasResources(biome))
                {
                    this.Resources.Add(position, this.SelectResource(rng, biome));
                }
            }
        }
    }
}