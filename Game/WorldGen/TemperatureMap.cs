using System;
using System.Reflection.Metadata;
using Engine.Core;
using Engine.Graphics;
using Game.Maths;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;
using Range = Game.Maths.FloatRange;

namespace Game.WorldGen
{
    /// <summary>
    /// Represents the temperature layer of a game world
    /// </summary>
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
        /// Threshold under which temperature is coldest
        /// </summary>
        public float ColdestThreshold { get; protected set; }
        
        /// <summary>
        /// Threshold under which temperature is colder
        /// </summary>
        public float ColderThreshold { get; protected set; }
        
        /// <summary>
        /// Threshold under which temperature is cold
        /// </summary>
        public float ColdThreshold { get; protected set; }
        
        /// <summary>
        /// Threshold under which temperature is warm
        /// </summary>
        public float WarmThreshold { get; protected set; }
        
        /// <summary>
        /// Threshold under which temperature is warmer
        /// </summary>
        public float WarmerThreshold { get; protected set; }

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
            var rng = new Random(this.Seed + 1443285);

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

            this.ColdestThreshold = this.CalculateThreshold(this.Parameters.ColdestPercentage);
            this.ColderThreshold = this.CalculateThreshold(this.Parameters.ColderPercentage);
            this.ColdThreshold = this.CalculateThreshold(this.Parameters.ColdPercentage);
            this.WarmThreshold = this.CalculateThreshold(this.Parameters.WarmPercentage);
            this.WarmerThreshold = this.CalculateThreshold(this.Parameters.WarmerPercentage);

            var mapper = new ValueMapper
            {
                { this.ColdestThreshold, 0.15f },
                { this.ColderThreshold, 0.35f },
                { this.ColdThreshold, 0.55f },
                { this.WarmThreshold, 0.75f },
                { this.WarmerThreshold, 0.85f }
            };

            // Helper variables for glacier limiting
            var glacierLimitLower = (float)this.Dimensions.Height * this.Parameters.ColdZoneLongitudeLimit;
            var glacierLimitUpper = (float)this.Dimensions.Height * this.Parameters.ColdZoneLongitudeLimit * 0.75f;
            var glacierSrcRange = new FloatRange(glacierLimitUpper, glacierLimitLower);
            var glacierDestRange = new FloatRange(0.0f, 1.0f);

            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var temperature = this.Values[ix, iy];
                    
                    var temperatureLevel = TemperatureLevel.Warmest;

                    if (temperature <= this.ColdestThreshold)
                        temperatureLevel = TemperatureLevel.Coldest;
                    else if (temperature <= this.ColderThreshold)
                        temperatureLevel = TemperatureLevel.Colder;
                    else if (temperature <= this.ColdThreshold)
                        temperatureLevel = TemperatureLevel.Cold;
                    else if (temperature <= this.WarmThreshold)
                        temperatureLevel = TemperatureLevel.Warm;
                    else if (temperature <= this.WarmerThreshold)
                        temperatureLevel = TemperatureLevel.Warmer;

                    this.Values[ix, iy] = mapper.Map(temperature);

                    

                    if (this.Parameters.LimitColdZones
                        && (iy >= (int)glacierLimitUpper)
                        && (temperatureLevel == TemperatureLevel.Colder ||
                            temperatureLevel == TemperatureLevel.Coldest))
                    {
                        // Determine distance from "a bit over the limit" to limit within [0, 1].
                        // We want to apply a smooth gradient here and use that as a probability
                        // to create a much less harsh edge, while still making sure the glacier never goes over the
                        // limit                     
                        var prob = MathUtil.Map((float)iy, glacierSrcRange, glacierDestRange);
                        prob = MathUtil.Smoothstep(prob);

                        if(rng.NextDouble() <= prob)
                        {
                            temperatureLevel = TemperatureLevel.Cold;
                            this.Values[ix, iy] = 0.45f;
                        }
                    }
                    
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