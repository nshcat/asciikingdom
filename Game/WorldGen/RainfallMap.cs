using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using Engine.Core;
using Engine.Graphics;
using Game.Maths;
using OpenToolkit.Graphics.ES11;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;
using Range = Game.Maths.Range;

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
        protected float NoiseWeight { get; } = 0.085f;//0.20f;
        
        /// <summary>
        /// Moisture from rivers
        /// </summary>
        protected float[,] IrrigationValues { get; }
        
        /// <summary>
        /// The rain shadow values
        /// </summary>
        protected float[,] RainShadow { get; }

        /// <summary>
        /// With how much rain the clouds start with
        /// </summary>
        protected float InitialRainAmount { get; } = 0.60f;

        /// <summary>
        /// The maximum rain amount a cloud can carry
        /// </summary>
        protected float MaxRainAmount { get; } = 1.0f;

        /// <summary>
        /// Minimum amount of rain possible
        /// </summary>
        protected float MinRainAmount { get; } = 0.15f;

        /// <summary>
        /// The max rain loss rate (at peak elevation)
        /// </summary>
        protected float MaxRainLoss { get; } = 0.2f;//0.25f;
        
        /// <summary>
        /// The min rain loss rate (at sea level)
        /// </summary>
        protected float MinRainLoss { get; } = 0.15f;

        /// <summary>
        /// Rain loss over mountains
        /// </summary>
        protected float MountainRainLoss { get; } = 0.30f;

        /// <summary>
        /// How much rain to accumulate over water. Will be multiplied onto the current rain amount.
        /// </summary>
        protected float RainGain { get; } = 1.15f; // 15% increase per tile

        /// <summary>
        /// A flat amount of rain clouds accumulate every time they are over normal land
        /// </summary>
        protected float FlatRainGain { get; } = 0.2f;

        /// <summary>
        /// Weight of the rain shadow in the final rainfall map
        /// </summary>
        protected float RainShadowWeight { get; } = 0.85f;

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
        /// Reference to the river generator. Used to check if certain tiles are rivers.
        /// </summary>
        protected RiverGenerator Rivers { get; set; }

        /// <summary>
        /// Construct new rainfall map
        /// </summary>
        public RainfallMap(Size dimensions, int seed, WorldParameters parameters, HeightMap elevation, RiverGenerator rivers)
            : base(dimensions, seed, parameters)
        {
            this.Elevation = elevation;
            this.Rivers = rivers;
            this.RainfallTiles = new Tile[dimensions.Width, dimensions.Height];
            this.RainShadow = new float[dimensions.Width, dimensions.Height];
            this.NoiseValues = new float[dimensions.Width, dimensions.Height];
            this.IrrigationValues = new float[dimensions.Width, dimensions.Height];
            this.Generate();
        }

        /// <summary>
        /// Generate rainfall map
        /// </summary>
        protected void Generate()
        {
            this.GenerateShadow();
            this.GenerateNoise();
            this.GenerateIrrigation();
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
            for (var iy = 0; iy < this.Dimensions.Height; ++iy)
            {
                   this.GenerateShadowLine(iy);
            }
            
            this.Normalize(this.Values);
        }

        /// <summary>
        /// Generate rain shadow for given latitude line
        /// </summary>
        protected void GenerateShadowLine(int y)
        {
            var currentRain = this.InitialRainAmount;
            var sourceRange = new Range(this.Elevation.SeaThreshold, this.Elevation.LandThreshold);
            var destRange = new Range(this.MinRainLoss, this.MaxRainLoss);

            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                // Retrieve elevation
                var elevation = this.Elevation[ix, y];
                
                // Are we over water?
                if (elevation < this.Elevation.SeaThreshold)
                {
                    // Accumulate more rain by evaporation of ocean water
                    currentRain *= this.RainGain;
                    currentRain = Math.Clamp(currentRain, this.MinRainAmount, this.MaxRainAmount);
                }
                else if (elevation <= this.Elevation.LandThreshold) // Are we over land?
                {
                    // Percentage of how much water is lost from the cloud and rained down
                    var factor = MathUtil.Map(elevation, sourceRange, destRange);
                    var loss = currentRain * factor;
                    currentRain *= 1.0f - factor;
                    this.RainShadow[ix, y] = loss;
                    
                    // This isnt realistic, but to make maps less weird looking we add a fixed amount of water to the cloud
                    // each tile it travels.
                    currentRain += this.FlatRainGain; 
                    
                    if (currentRain < this.MinRainAmount)
                        currentRain = this.MinRainAmount;
                }
                else
                {
                    // Loose rain at max rate
                    var loss = currentRain * this.MountainRainLoss;
                    currentRain *= 1.0f - this.MountainRainLoss;
                    this.RainShadow[ix, y] = loss; // DEBUG remove this, because it skews the threshold calculations
                    
                    if (currentRain < this.MinRainAmount)
                        currentRain = this.MinRainAmount;
                }
            }
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
                    if (this.Elevation[ix, iy] < this.Elevation.SeaThreshold)
                        this.Values[ix, iy] = 0.0f;
                    else
                    {
                        var rainfall = this.RainShadowWeight * this.RainShadow[ix, iy]
                                       + this.NoiseWeight * this.NoiseValues[ix, iy]
                                       + this.IrrigationValues[ix, iy];

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
        /// Generate irrigation values from rivers
        /// </summary>
        protected void GenerateIrrigation()
        {
            if (!this.Parameters.RiverIrrigation)
                return;
            
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    // Check if its a river
                    var position = new Position(ix, iy);
                    if (this.Rivers.IsInAnyRiver(position))
                    {
                        // Irrigate in a 3x3
                        for (var dx = -5; dx <= 5; ++dx)
                        {
                            for (var dy = -5; dy <= 5; ++dy)
                            {
                                var irrigationPos = position + new Position(dx, dy);
                                
                                if(irrigationPos.X < 0 || irrigationPos.X >= this.Dimensions.Width
                                || irrigationPos.Y < 0 || irrigationPos.Y >= this.Dimensions.Height)
                                    continue;

                                Func<float, float> falloff = x => 1.0f - (float)(Math.Pow(0.08f, x) - 1.0f) / (0.08f - 1.0f);

                                var baseAmount = 0.35f; // 0.15f
                                var amountX = baseAmount * falloff(Math.Abs(dx) / 5.0f);
                                var amountY = baseAmount * falloff(Math.Abs(dy) / 5.0f);
                                var amount = Math.Min(amountX, amountY);

                                this.IrrigationValues[irrigationPos.X, irrigationPos.Y]
                                    = Math.Max(this.IrrigationValues[irrigationPos.X, irrigationPos.Y], amount);
                            }
                        }
                    }
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