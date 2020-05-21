using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Numerics;
using Engine.Core;
using Engine.Graphics;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;

namespace Game.WorldGen
{
    /// <summary>
    /// Represents the rainfall layer of the world
    /// </summary>
    public class RainfallMapAlt : Map
    {
        /// <summary>
        /// The elevation layer
        /// </summary>
        protected HeightMap Elevation { get; set; }
        
        /// <summary>
        /// The temperature layer
        /// </summary>
        protected TemperatureMap Temperature { get; set; }
        
        /// <summary>
        /// Rainfall layer rendered as tiles
        /// </summary>
        public Tile[,] RainfallTiles { get; protected set; }

        /// <summary>
        /// Color used for low end of rainfall values
        /// </summary>
        protected Color LowColor { get; } = DefaultColors.White;
        
        /// <summary>
        /// Color used for high end of rainfall values
        /// </summary>
        protected Color HighColor { get; } = new Color(0, 60, 255);
        
        /// <summary>
        /// Noise map used to distort rainfall a bit
        /// </summary>
        protected float[,] NoiseValues { get; }

        /// <summary>
        /// How strongly the noise is weighted when combining it with the rainfall
        /// </summary>
        protected float NoiseWeight { get; } = 0.20f;

        /// <summary>
        /// Threshold under which the world is very dry
        /// </summary>
        public float BarrenThreshold { get; protected set; }
        
        /// <summary>
        /// Threshold under which grass-like biomes can exist
        /// </summary>
        public float GrassThreshold { get; protected set; }
        
        /// <summary>
        /// Threshold under which conifers can exist
        /// </summary>
        public float ConiferThreshold { get; protected set; }
        
        /// <summary>
        /// Construct new rainfall map
        /// </summary>
        public RainfallMapAlt(Size dimensions, int seed, WorldParameters parameters, HeightMap elevation, TemperatureMap temperature)
            : base(dimensions, seed, parameters)
        {
            this.Elevation = elevation;
            this.Temperature = temperature;
            this.RainfallTiles = new Tile[dimensions.Width, dimensions.Height];
            this.NoiseValues = new float[dimensions.Width, dimensions.Height];
            this.Generate();
        }

        /// <summary>
        /// Generate rainfall map
        /// </summary>
        protected void Generate()
        {
            this.GenerateShadow();
            this.GenerateNoise();
            this.GenerateRainfall();
            this.GenerateTiles();
        }

        /// <summary>
        /// Generate noise component
        /// </summary>
        protected void GenerateNoise()
        {
            var module = new Perlin()
            {
                Seed = this.Seed + 44122,
                Frequency = 0.5,
                Lacunarity = 2.75,
                Persistence = 1.0,
                OctaveCount = 6
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
        /// Generate rain component
        /// </summary>
        protected void GenerateShadow()
        {
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    /*this.Values[ix, iy] =
                        (Math.Clamp(this.Elevation[ix, iy] - (this.Elevation.SeaThreshold), 0.0f, 1.0f))
                        * (1.0f - this.Temperature[ix, iy]);*/
                    this.Values[ix, iy] =
                        (Math.Clamp(this.Elevation[ix, iy] - (this.Elevation.SeaThreshold), 0.0f, 1.0f))
                        * (1.0f - this.Temperature[ix, iy] * 0.36f);

                    if (this.Elevation[ix, iy] > this.Elevation.SeaThreshold)
                    {
                        var distance = this.Elevation[ix, iy] - this.Elevation.SeaThreshold;
                        this.Values[ix, iy] += (1.0f - distance) * 0.35f;
                    }
                }
            }
            
            this.Normalize(this.Values);
        }
        
        /// <summary>
        /// Combine rain shadow and noise
        /// </summary>
        protected void GenerateRainfall()
        {
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    if (this.Elevation[ix, iy] <= this.Elevation.SeaThreshold)
                        this.Values[ix, iy] = 0.0f;
                    else
                    {
                        var rainfall = 0.45f * this.Values[ix, iy]
                                       + 0.65f * this.NoiseValues[ix, iy];

                        this.Values[ix, iy] = rainfall;
                    }
                }
            }

            this.Normalize(this.Values);

            this.BarrenThreshold = this.CalculateThreshold(this.Parameters.BarrenPercentage, true);
            this.GrassThreshold = this.CalculateThreshold(this.Parameters.GrassPercentage, true);
            this.ConiferThreshold = this.CalculateThreshold(this.Parameters.ConiferPercentage, true);
            
            var mapper = new ValueMapper
            {
                {this.BarrenThreshold, 0.09f},
                {this.GrassThreshold, 0.65f},
                {this.ConiferThreshold, 0.88f}
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
        /// Generate tile rendering of rainfall values
        /// </summary>
        protected void GenerateTiles()
        {
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var rainfall = this.Values[ix, iy];
                    var color = rainfall == 0.0f ? DefaultColors.Black : Color.Lerp(rainfall, this.LowColor, this.HighColor);
                    var tile = new Tile(0, DefaultColors.Black, color);
                    this.RainfallTiles[ix, iy] = tile;
                }
            }
        }
    }
}