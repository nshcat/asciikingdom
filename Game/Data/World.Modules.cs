using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Models;
using SharpNoise.Models;
using SharpNoise.Modules;

namespace Game.Data
{
    public partial class World
    {
        /// <summary>
        /// Build the noise module tree used for temperature map generation
        /// </summary>
        private static Module BuildTemperatureModuleTree(int seed)
        {
            var terrainType1 = new Perlin()
            {
                Frequency = 0.6,
                Persistence = 0.8,
                Lacunarity = 2.5,
                OctaveCount = 4,
                Seed = seed
            };

            return terrainType1;
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

    }
}