using System;
using System.Reflection.Metadata;
using System.Xml;
using Engine.Core;
using Engine.Graphics;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;

namespace Game.WorldGen
{
    /// <summary>
    /// Represents the drainage layer of the world
    /// </summary>
    public class DrainageMap : Map
    {
        /// <summary>
        /// The elevation map
        /// </summary>
        protected HeightMap Elevation { get; set; }
        
        /// <summary>
        /// Elevation map standard derivation
        /// </summary>
        protected float[,] StandardDerivation { get; set; }
        
        /// <summary>
        /// Noise values used to make drainage map less regular looking
        /// </summary>
        protected float[,] NoiseValues { get; set; }

        /// <summary>
        /// The filter size to use to compute the standard derivation of the elevation map
        /// </summary>
        protected int FilterSize { get; set; } = 16;

        /// <summary>
        /// Weight factor for the standard derivation in the final drainage values
        /// </summary>
        protected float StandardDerivationWeight { get; set; } = 0.75f;

        /// <summary>
        /// Weight factor for the noise in the final drainage values
        /// </summary>
        protected float NoiseWeight { get; set; } = 0.75f;

        /// <summary>
        /// Color for low drainage
        /// </summary>
        protected Color LowColor { get; set; } = new Color(98, 112, 17);
        
        /// <summary>
        /// Color for high drainage
        /// </summary>
        protected Color HighColor { get; set; } = new Color(203, 213, 147);
        
        /// <summary>
        /// Visualization of the drainage levels
        /// </summary>
        public Tile[,] DrainageTiles { get; protected set; }
        
        /// <summary>
        /// Threshold under which drainage falls into range [0, 0.32]
        /// </summary>
        public float DesertThreshold { get; protected set; }
        
        /// <summary>
        /// Threshold under which drainage falls into range [0, 0.49]
        /// </summary>
        public float RockyThreshold { get; protected set; }
        
        /// <summary>
        /// Threshold under which drainage falls into range [0, 0.65]
        /// </summary>
        public float HillsThreshold { get; protected set; }
        
        /// <summary>
        /// Construct new drainage map
        /// </summary>
        public DrainageMap(Size dimensions, int seed, WorldParameters parameters, HeightMap heightMap)
            : base(dimensions, seed, parameters)
        {
            this.Elevation = heightMap;
            this.StandardDerivation = new float[dimensions.Width, dimensions.Height];
            this.NoiseValues = new float[dimensions.Width, dimensions.Height];
            this.DrainageTiles = new Tile[dimensions.Width, dimensions.Height];
            this.Generate();
        }

        /// <summary>
        /// Checks whether given coordinate pair is within the map bounds.
        /// </summary>
        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < this.Dimensions.Width && y >= 0 && y < this.Dimensions.Height;
        }

        /// <summary>
        /// Generate the drainage map
        /// </summary>
        private void Generate()
        {
            this.GenerateStandardDerivation();
            this.GenerateNoise();
            this.GenerateDrainage();
            this.GenerateTiles();
        }

        /// <summary>
        /// Generate tile visualization
        /// </summary>
        private void GenerateTiles()
        {
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var drainage = this.Values[ix, iy];
                    var color = drainage == 0.0f ? DefaultColors.Black : Color.Lerp(drainage, this.LowColor, this.HighColor);
                    var tile = new Tile(0, DefaultColors.Black, color);
                    this.DrainageTiles[ix, iy] = tile;
                }
            }
        }

        /// <summary>
        /// Generate the actual drainage values by combining noise and standard derivation
        /// </summary>
        private void GenerateDrainage()
        {
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    // Drainage under water is just 0.0
                    if (this.Elevation[ix, iy] <= this.Elevation.SeaThreshold ||
                        this.Elevation[ix, iy] >= this.Elevation.LandThreshold)
                        this.Values[ix, iy] = 0.0f;
                    else
                    {
                        var drainage = this.StandardDerivationWeight * this.StandardDerivation[ix, iy]
                                       + this.NoiseWeight * this.NoiseValues[ix, iy];

                        this.Values[ix, iy] = drainage;
                    }
                }
            }

            this.Normalize(this.Values);

            this.DesertThreshold = this.CalculateThreshold(this.Parameters.DesertPercentage, true);
            this.RockyThreshold = this.CalculateThreshold(this.Parameters.RockyPercentage, true);
            this.HillsThreshold = this.CalculateThreshold(this.Parameters.HillsPercentage, true);

            var mapper = new ValueMapper
            {
                {this.DesertThreshold, 0.32f},
                {this.RockyThreshold, 0.49f},
                {this.HillsThreshold, 0.65f}
            };

            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    this.Values[ix, iy] = mapper.Map(this.Values[ix, iy]);
                }
            }
        }

        /// <summary>
        /// Generate noise values
        /// </summary>
        private void GenerateNoise()
        {
            var module = new Perlin()
            {
                Seed = this.Seed + 12842,
                Frequency = 24,
                OctaveCount = 5
            };
            
            var bounds = new System.Drawing.RectangleF(4, 1, 1, 1);
            var noiseMap = new NoiseMap();
            var builder = new PlaneNoiseMapBuilder()
            {
                DestNoiseMap = noiseMap,
                SourceModule = module
            };
            builder.SetDestSize(this.Dimensions.Width, this.Dimensions.Height);

            builder.SetBounds(bounds.Left, bounds.Right, bounds.Top, bounds.Bottom);
            builder.Build();
            
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    this.NoiseValues[ix, iy] = noiseMap[ix, iy];
                }
            }
            
            this.Normalize(this.NoiseValues);
        }
        
        /// <summary>
        /// Generate the standard derivation map
        /// </summary>
        private void GenerateStandardDerivation()
        {
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var sum = 0.0f;
                    var count = 0;
                    for (var ixx = 0; ixx < this.FilterSize; ++ixx)
                    {
                        for (var iyy = 0; iyy < this.FilterSize; ++iyy)
                        {
                            int x = ix - this.FilterSize / 2 + ixx;
                            int y = iy - this.FilterSize / 2 + iyy;

                            if (this.IsInBounds(x, y))
                            {
                                count++;
                                var elevation = this.Elevation[x, y];

                                if (elevation > this.Elevation.SeaThreshold)
                                    sum += elevation;
                                else
                                    sum += this.Elevation.SeaThreshold;
                            }
                        }
                    }

                    sum /= count;

                    var derivation = 0.0f;
                    for (var ixx = 0; ixx < this.FilterSize; ++ixx)
                    {
                        for (var iyy = 0; iyy < this.FilterSize; ++iyy)
                        {
                            int x = ix - this.FilterSize / 2 + ixx;
                            int y = iy - this.FilterSize / 2 + iyy;
                            
                            if (this.IsInBounds(x, y))
                            {
                                var tmp = 0.0f;
                                
                                var elevation = this.Elevation[x, y];

                                if (elevation > this.Elevation.SeaThreshold)
                                    tmp = elevation - sum;
                                else
                                    tmp = this.Elevation.SeaThreshold - sum;

                                derivation += tmp * tmp;
                            }
                        }
                    }

                    derivation = (float)Math.Sqrt((double)(derivation / (float) count));
                    this.StandardDerivation[ix, iy] = derivation;
                }
            }
            
            this.Normalize(this.StandardDerivation);
        }
    }
}