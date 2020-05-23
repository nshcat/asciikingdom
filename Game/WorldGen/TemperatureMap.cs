using System;
using Engine.Core;
using Engine.Graphics;
using Game.Maths;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;
using Range = Game.Maths.Range;

namespace Game.WorldGen
{
    /// <summary>
    /// Represents the temperature layer of a game world
    /// </summary>
    /// TODO: Maybe also use percentages here to make fixed amount of glacier?
    /// Own temperature type for glacier?
    public class TemperatureMap : Map
    {
        /// <summary>
        /// The temperature layer in the form of temperature level values
        /// </summary>
        public TemperatureLevel[,] TemperatureLevels { get; protected set; }
        
        /// <summary>
        /// Temperature layer rendered as tiles
        /// </summary>
        public Tile[,] TemperatureTiles { get; protected set; }
        
        /// <summary>
        /// The height map of the world
        /// </summary>
        protected HeightMap Elevation { get; set; }
        
        /// <summary>
        /// The temperature gradient based on longitude
        /// </summary>
        protected float[,] Gradient { get; }

        /// <summary>
        /// Weight of the gradient in the final temperature map
        /// </summary>
        protected float GradientWeight { get; } = 1.0f;
        
        /// <summary>
        /// Noise values used to distort the temperature gradient
        /// </summary>
        protected float[,] NoiseValues { get; }

        /// <summary>
        /// Weight of the noise in the final temperature map
        /// </summary>
        protected float NoiseWeight { get; } = 0.25f;

        /// <summary>
        /// Create new temperature map
        /// </summary>
        public TemperatureMap(Size dimensions, int seed, WorldParameters parameters, HeightMap elevation)
            : base(dimensions, seed, parameters)
        {
            this.TemperatureLevels = new TemperatureLevel[dimensions.Width, dimensions.Height];
            this.TemperatureTiles = new Tile[dimensions.Width, dimensions.Height];
            this.NoiseValues = new float[dimensions.Width, dimensions.Height];
            this.Gradient = new float[dimensions.Width, dimensions.Height];
            this.Elevation = elevation;
            
            this.Generate();
        }

        /// <summary>
        /// Generate the gradient based on longitude
        /// </summary>
        private void GenerateGradient()
        {
            for (var iy = 0; iy < this.Dimensions.Height; ++iy)
            {
                var temperature = ((float) iy) / this.Dimensions.Height;

                for (var ix = 0; ix < this.Dimensions.Width; ++ix)
                {
                    this.Gradient[ix, iy] = temperature;
                }
            }
        }

        /// <summary>
        /// Generate the noise values
        /// </summary>
        private void GenerateNoise()
        {
            var module = new Perlin()
            {
                Seed = this.Seed + 333211,
                Frequency = 20.0,
                OctaveCount = 2
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
        /// Generate the temperature map
        /// </summary>
        private void Generate()
        {
            this.GenerateGradient();
            this.GenerateNoise();

            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    this.Values[ix, iy] = this.GradientWeight * this.Gradient[ix, iy] + this.NoiseWeight * this.NoiseValues[ix, iy];
                }
            }
            
            this.Normalize();

            var coldestThreshold = this.CalculateThreshold(this.Parameters.ColdestPercentage);
            var colderThreshold = this.CalculateThreshold(this.Parameters.ColderPercentage);
            var coldThreshold = this.CalculateThreshold(this.Parameters.ColdPercentage);
            var warmThreshold = this.CalculateThreshold(this.Parameters.WarmPercentage);
            var warmerThreshold = this.CalculateThreshold(this.Parameters.WarmerPercentage);

            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var temperature = this.Values[ix, iy];
                    
                    var temperatureLevel = TemperatureLevel.Warmest;

                    if (temperature <= coldestThreshold)
                        temperatureLevel = TemperatureLevel.Coldest;
                    else if (temperature <= colderThreshold)
                        temperatureLevel = TemperatureLevel.Colder;
                    else if (temperature <= coldThreshold)
                        temperatureLevel = TemperatureLevel.Cold;
                    else if (temperature <= warmThreshold)
                        temperatureLevel = TemperatureLevel.Warm;
                    else if (temperature <= warmerThreshold)
                        temperatureLevel = TemperatureLevel.Warmer;

                    this.TemperatureLevels[ix, iy] = temperatureLevel;
                    var tile = new Tile(0, DefaultColors.Black, this.GetTemperatureColor(temperatureLevel));
                    this.TemperatureTiles[ix, iy] = tile;
                }
            }
        }
        
        /// <summary>
        /// Retrieve color for given temperature levels
        /// </summary>
        private Color GetTemperatureColor(TemperatureLevel level)
        {
            switch (level)
            {
                case TemperatureLevel.Coldest:
                    return new Color(0, 255, 255);
                case TemperatureLevel.Colder:
                    return new Color(170, 255, 255);
                case TemperatureLevel.Cold:
                    return new Color(0, 229, 133);
                case TemperatureLevel.Warm:
                    return new Color(255, 255, 100);
                case TemperatureLevel.Warmer:
                    return new Color(255, 100, 0);
                default:
                    return new Color(241, 12, 0);
            }
        }
    }
}