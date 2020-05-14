using System;
using System.ComponentModel;
using Engine.Core;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;

namespace Game.WorldGen
{
    /// <summary>
    /// Represents the elevation layer of the world.
    /// </summary>
    internal class HeightMap: Map
    {
        /// <summary>
        /// Elevation of the map, in the form of height level values
        /// </summary>
        public HeightLevel[,] HeightLevels { get; protected set; }

        /// <summary>
        /// Index operator implementation, which just redirects to the internal
        /// array
        /// </summary>
        public float this[int x, int y] => this.Values[x, y];

        /// <summary>
        /// Create a new height map.
        /// </summary>
        public HeightMap(Size dimensions, int seed, WorldParameters parameters)
            : base(dimensions, seed, parameters)
        {
            this.HeightLevels = new HeightLevel[dimensions.Width, dimensions.Height];
            this.Generate();
        }
        
        /// <summary>
        /// Accentuate mountain peaks by squaring all height values.
        /// </summary>
        private void AccentuatePeaks()
        {
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    this.Values[ix, iy] = (float) Math.Pow(this.Values[ix, iy], 2.0);
                }
            }
        }

        /// <summary>
        /// Generate the height map
        /// </summary>
        private void Generate()
        {
            var bounds = new System.Drawing.RectangleF(6, 1, 3, 3);
            var noiseModule = this.BuildModuleTree();
            var noiseMap = new NoiseMap();
            var builder = new PlaneNoiseMapBuilder()
            {
                DestNoiseMap = noiseMap,
                SourceModule = noiseModule
            };
            builder.SetDestSize(this.Dimensions.Width, this.Dimensions.Height);

            builder.SetBounds(bounds.Left, bounds.Right, bounds.Top, bounds.Bottom);
            builder.Build();
            
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    this.Values[ix, iy] = noiseMap[ix, iy];
                }
            }
            
            this.Normalize();
            this.AccentuatePeaks();
            this.Normalize();
            this.DetermineHeightLevels();
        }
        
        /// <summary>
        /// Build the noise tree used for terrain generation
        /// </summary>
        private Module BuildModuleTree()
        {
            var mountainTerrain = new RidgedMulti()
            {
                Seed = this.Seed
            };

            var baseFlatTerrain = new Billow()
            {
                Seed = this.Seed,
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
                Seed = this.Seed
            };
            
            var terrainType2 = new Perlin()
            {
                Frequency = 0.5,
                Persistence = 0.25,
                Seed = this.Seed + 1337
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
                Seed = this.Seed
            };

            return finalTerrain;
        }
        
        /// <summary>
        /// Refine elevation map and determine height level values
        /// </summary>
        private void DetermineHeightLevels()
        {
            // TODO: For drainage, we need the final elevation values in [0, 1]. Therefore,
            // this function should be renamed into RefineHeightmap and return both the height levels
            // as well as a 2D float array with remapped elevation values corresponding to the various
            // height levels. Do we need to actually remap them? Or is it enough if the drainage generator
            // knows the sea and low hill thresholds? That might be enough!

            var lowMountainPercent =
                this.Parameters.TreeLinePercentage + (1.0f - this.Parameters.TreeLinePercentage) / 2.0f;
            var medMountainPercent =
                lowMountainPercent + (1.0f - lowMountainPercent) / 1.8f;
            
            var highMountainPercent =
                medMountainPercent + (1.0f - medMountainPercent) / 1.006f;
            
            var seaThreshold = this.CalculateThreshold(this.Parameters.UnderWaterPercentage);
            var treeLineThreshold = this.CalculateThreshold(this.Parameters.TreeLinePercentage);
            var lowMountainThreshold = this.CalculateThreshold(lowMountainPercent);
            var medMountainThreshold = this.CalculateThreshold(medMountainPercent);
            var highMountainThreshold = this.CalculateThreshold(highMountainPercent);
            
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var heightValue = this.Values[ix, iy];
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

                    this.HeightLevels[ix, iy] = heightLevel;
                }
            }
        }
    }
}