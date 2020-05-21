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
    public class RainfallMap : Map
    {
        /// <summary>
        /// The elevation layer
        /// </summary>
        protected HeightMap Elevation { get; set; }
        
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
        /// All clouds used in the rainfall generation
        /// </summary>
        protected List<Cloud> Clouds { get; } = new List<Cloud>();

        /// <summary>
        /// How many different cross wind angles will be used
        /// </summary>
        protected int CrossWindCount { get; } = 10;

        /// <summary>
        /// The angle cone where all cross winds are contained within
        /// </summary>
        protected float CrossWindCone { get; } = (float)Math.PI; // * 13.0f / 20.0f;

        /// <summary>
        /// Initial wind angle
        /// </summary>
        protected float WindAngle { get; } = 0.0f;
        
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
        public RainfallMap(Size dimensions, int seed, WorldParameters parameters, HeightMap elevation)
            : base(dimensions, seed, parameters)
        {
            this.Elevation = elevation;
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
                Frequency = 1.4,
                Lacunarity = 1.75,
                Persistence = 0.5,
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
            for (var i = 0; i < this.CrossWindCount; ++i)
            {
                var angle = this.WindAngle - this.CrossWindCone +
                         (float) i * (2.0f * this.CrossWindCone / (float) this.CrossWindCount);

                var rainWeight = (this.CrossWindCone - Math.Abs(this.WindAngle - angle)) / this.CrossWindCone;
                
                if(rainWeight == 0.0f)
                    continue;
                
                this.SetupClouds(angle);
                var direction = this.VectorFromAngle(angle);

                while (this.Clouds.Count > 0)
                {
                    for (var j = 0; j < this.Clouds.Count; ++j)
                    {
                        var cloud = this.Clouds[j];
                        cloud.Move(direction);

                        if (cloud.X < 0.0f || cloud.X >= this.Dimensions.Width ||
                            cloud.Y < 0.0f || cloud.Y >= this.Dimensions.Height)
                        {
                            this.Clouds.RemoveAt(j);
                        }
                        else
                        {
                            cloud.DropRain(rainWeight);
                        }
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
                        var rainfall = this.Values[ix, iy]
                                       + this.NoiseWeight * this.NoiseValues[ix, iy];

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

        /// <summary>
        /// Set up all clouds for given wind angle
        /// </summary>
        /// <param name="angle"></param>
        protected void SetupClouds(float angle)
        {
            this.Clouds.Clear();
            
            var direction = this.VectorFromAngle(angle);
            var v1 = (direction.X < 0.0f) ? this.Dimensions.Width - 1 : 0;
            var v2 = (direction.Y < 0.0f) ? this.Dimensions.Height - 1 : 0;

            var v3 = Math.Abs(direction.X / direction.Y);
            var v4 = Math.Abs(direction.Y / direction.X);

            if (float.IsNaN(v3) || v3 > (float) this.Dimensions.Width)
                v3 = (float) this.Dimensions.Width + 1.0f;
            else if (v3 < 1.0f)
                v3 = 1.0f;

            if (float.IsNaN(v4) || v4 > (float) this.Dimensions.Height)
                v4 = (float) this.Dimensions.Height + 1.0f;
            else if (v4 < 1.0f)
                v4 = 1.0f;

            var v5 = (int) v3;
            var v6 = (int) v4;

            for (var ix = v5 - 1; ix < this.Dimensions.Width; ix += v5)
            {
                this.Clouds.Add(new Cloud(this.Elevation, this, new Position(ix, v2)));
            }

            for (var iy = v6; iy < this.Dimensions.Height - 1; iy += v6)
            {
                this.Clouds.Add(new Cloud(this.Elevation, this, new Position(v1, iy)));
            }
        }

        /// <summary>
        /// Get unit vector based on given angle
        /// </summary>
        protected Vector2 VectorFromAngle(float angle)
        {
            var x = (float)-Math.Cos(angle + Math.PI / 2.0);
            var y = (float)-Math.Cos(angle);
            return new Vector2(x, y);
        }
    }
}