using System;
using Engine.Core;
using OpenToolkit.Graphics.ES20;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;

namespace Game.WorldGen
{
    /// <summary>
    /// Represents the elevation layer of the world.
    /// </summary>
    public class HeightMap: Map
    {
        /// <summary>
        /// Elevation of the map, in the form of height level values
        /// </summary>
        public HeightLevel[,] HeightLevels { get; protected set; }

        /// <summary>
        /// Percentage of map that is masked off at both the western and eastern ends of the map
        /// </summary>
        protected float MaskPercentage { get; } = 0.08f;

        /// <summary>
        /// Elevation value marking the sea level
        /// </summary>
        public float SeaThreshold { get; protected set; }

        /// <summary>
        /// Elevation value marking the border between land and mountains
        /// </summary>
        public float LandThreshold { get; protected set; }

        /// <summary>
        /// Elevation value marking the border between low and medium sized mountains
        /// </summary>
        public float LowMountainThreshold { get; protected set; }
        
        /// <summary>
        /// Elevation value marking the border between medium and high sized mountains
        /// </summary>
        public float MediumMountainThreshold { get; protected set; }
        
        /// <summary>
        /// Elevation value marking the border between high sized mountains and mountain peaks
        /// </summary>
        public float HighMountainThreshold { get; protected set; }

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
        /// Fill all sinks in the map using a depression filling algorithm.
        /// This causes each map tile to have at least one neighbouring cell with a
        /// lower elevation.
        /// </summary>
        private void FillSinks()
        {
            // This RNG is only used when randomized sink filling is enabled
            var rng = new Random(this.Seed + 17710);
            
            var directions = new[]
            {
                Position.East,
                Position.North, 
                Position.South,
                Position.West
            };

            var newValues = new float[this.Dimensions.Width, this.Dimensions.Height];
            var epsilon = 1e-5f;
            var infinity = 999999.0f;

            Func<int, int, bool> isNearEdge = (x, y) =>
                (x == 0 || x == this.Dimensions.Width - 1) ||
                (y == 0 || y == this.Dimensions.Height - 1);

            // Set initial values
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    newValues[ix, iy] = isNearEdge(ix, iy) ? this.Values[ix, iy] : infinity;
                }
            }
            
            // Perform iteration
            while (true)
            {
                var wasChanged = false;

                // When randomized sink filling is enabled in the world parameters, we choose a random epsilon
                // for each iteration. This prevents weird artifacts from being generated (mostly completely straight,
                // very long diagonal lines, creating big diamond-shaped terrain patterns on some seeds)
                var currentEpsilon = this.Parameters.RandomizedSinkFilling
                    ? (float) (rng.NextDouble() * 1e-5f) + 1e-6f
                    : epsilon;

                for (var ix = 0; ix < this.Dimensions.Width; ++ix)
                {
                    for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                    {
                        if(this.Values[ix, iy] == newValues[ix, iy])
                            continue;

                        var position = new Position(ix, iy);
                        
                        // Check all neighbours
                        foreach (var direction in directions)
                        {
                            var neighbour = position + direction;

                            if (this.Values[ix, iy] >=
                                newValues[neighbour.X, neighbour.Y] + currentEpsilon)
                            {
                                newValues[ix, iy] = this.Values[ix, iy];
                                wasChanged = true;
                                break;
                            }

                            var oh = newValues[neighbour.X, neighbour.Y] + currentEpsilon;

                            if ((newValues[ix, iy] > oh) && (oh > this.Values[ix, iy]))
                            {
                                newValues[ix, iy] = oh;
                                wasChanged = true;
                            }
                        }
                    }
                }

                // The algorithm terminates when no changes were made
                if (!wasChanged)
                    break;
            }

            this.Values = newValues;
            this.Normalize();
        }

        /// <summary>
        /// Mask off eastern and western sides of the map to stop land from generating there
        /// </summary>
        private void MaskSides()
        {
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                var fx = (float) ix / this.Dimensions.Width;
                var weight = 1.0f;

                if (fx <= this.MaskPercentage)
                {
                    weight = fx / this.MaskPercentage;
                }
                else if (fx >= 1.0f - this.MaskPercentage)
                {
                    weight = 1.0f - ((fx - (1.0f - this.MaskPercentage)) / this.MaskPercentage);
                }

                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    this.Values[ix, iy] *= weight;
                }
            }
        }
        

        /// <summary>
        /// Generate the height map
        /// </summary>
        private void Generate()
        {
            //var bounds = new System.Drawing.RectangleF(6, 1, 3, 3);
            //var bounds = new System.Drawing.RectangleF(6, 1, 4, 4);
            var bounds = new System.Drawing.RectangleF(6, 1, 5, 5);
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

            if (this.Parameters.ForceOceanSides)
            {
                this.MaskSides();
            }

            if (this.Parameters.AccentuateHills)
            {
                this.AccentuatePeaks();
                this.Normalize();
            }
            
            this.FillSinks();
            
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
            var lowMountainPercent =
                this.Parameters.TreeLinePercentage + (1.0f - this.Parameters.TreeLinePercentage) / 3.0f;
            var medMountainPercent =
                lowMountainPercent + (1.0f - lowMountainPercent) / 1.8f;
            
            var highMountainPercent =
                medMountainPercent + (1.0f - medMountainPercent) / 1.006f;
            
            this.SeaThreshold = this.CalculateThreshold(this.Parameters.UnderWaterPercentage);
            this.LandThreshold = this.CalculateThreshold(this.Parameters.TreeLinePercentage);
            this.LowMountainThreshold = this.CalculateThreshold(lowMountainPercent);
            this.MediumMountainThreshold = this.CalculateThreshold(medMountainPercent);
            this.HighMountainThreshold = this.CalculateThreshold(highMountainPercent);
            
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var heightValue = this.Values[ix, iy];
                    var heightLevel = HeightLevel.MountainPeak;

                    if (heightValue <= this.SeaThreshold)
                        heightLevel = HeightLevel.Sea;
                    else if (heightValue <= this.LandThreshold)
                        heightLevel = HeightLevel.Land;
                    else if (heightValue <= this.LowMountainThreshold)
                        heightLevel = HeightLevel.LowMountain;
                    else if (heightValue <= this.MediumMountainThreshold)
                        heightLevel = HeightLevel.MediumMountain;
                    else if (heightValue <= this.HighMountainThreshold)
                        heightLevel = HeightLevel.HighMountain;

                    this.HeightLevels[ix, iy] = heightLevel;
                }
            }
        }
    }
}