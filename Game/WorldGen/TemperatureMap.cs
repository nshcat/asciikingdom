using System;
using Engine.Core;
using Engine.Graphics;
using Game.Maths;
using TinkerWorX.AccidentalNoiseLibrary;
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
        /// Create new temperature map
        /// </summary>
        public TemperatureMap(Size dimensions, int seed, WorldParameters parameters, HeightMap elevation)
            : base(dimensions, seed, parameters)
        {
            this.TemperatureLevels = new TemperatureLevel[dimensions.Width, dimensions.Height];
            this.TemperatureTiles = new Tile[dimensions.Width, dimensions.Height];
            this.Elevation = elevation;
            
            this.Generate();
        }

        /// <summary>
        /// Generate the temperature map
        /// </summary>
        private void Generate()
        {
            var dimensions = this.Dimensions;
            var gradient = new ImplicitGradient(1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1);

            var heatFractal = new ImplicitFractal(FractalType.Multi,
                BasisType.Simplex,
                InterpolationType.Quintic)
            {
                Frequency = 5.0,
                Gain = 1.6,
                Octaves = 5,
                Seed = this.Seed
            };

            var heatMap = new ImplicitCombiner(CombinerType.Multiply);
            heatMap.AddSource(gradient);
            heatMap.AddSource(heatFractal);

            // Noise range
            double x1 = 0, x2 = 2;
            double y1 = 0, y2 = 2;
            var dx = x2 - x1;
            var dy = y2 - y1;

            for (var ix = 0; ix < dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < dimensions.Height; ++iy)
                {
                    double u = ix / (float) dimensions.Width;
                    double v = iy / (float) dimensions.Height;

                    /*var nx = (float) (x1 + (u * dx));
                    var ny = (float) (y1 + (v * dy));
                    var temperatureValue = (float) heatMap.Get(nx, ny);*/
                    
                    var nx = (float) (x1 + Math.Cos(u * 2 * Math.PI) * dx / (2 * Math.PI));
                    var ny = (float) (y1 + Math.Cos(v * 2 * Math.PI) * dy / (2 * Math.PI));
                    var nz = (float) (x1 + Math.Sin(u * 2 * Math.PI) * dx / (2 * Math.PI));
                    var nw = (float) (y1 + Math.Sin(v * 2 * Math.PI) * dy / (2 * Math.PI));

                    var temperatureValue = (float) heatMap.Get(nx, ny, nz, nw);

                    this.Values[ix, iy] = temperatureValue;
                }
            }
            
            this.Normalize();

            var coldestThreshold = this.CalculateThreshold(this.Parameters.ColdestPercentage);
            var colderThreshold = this.CalculateThreshold(this.Parameters.ColderPercentage);
            var coldThreshold = this.CalculateThreshold(this.Parameters.ColdPercentage);
            var warmThreshold = this.CalculateThreshold(this.Parameters.WarmPercentage);
            var warmerThreshold = this.CalculateThreshold(this.Parameters.WarmerPercentage);

            for (var ix = 0; ix < dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < dimensions.Height; ++iy)
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

            /*var sourceRange = new Maths.Range(min, max);
            var destRange = new Maths.Range(0.0f, 1.0f);
            var heightSourceRange = new Range(0.4f, 1.0f);

            for (var ix = 0; ix < dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < dimensions.Height; ++iy)
                {
                    var value = MathUtil.Map(this.Values[ix, iy], sourceRange, destRange);
                    
                    // Factor in height
                    var height = this.Elevation[ix, iy];

                    if (height > 0.4f)
                    {
                        var scaled = MathUtil.Map(height, heightSourceRange, destRange);

                        value = Math.Clamp(value - (scaled * 0.35f /** 0.45f*/ /** 0.3f*/ /*), 0.0f, 1.0f);
                    }
                    
                   

                    this.TemperatureLevels[ix, iy] = level;
                    
                    var tile = new Tile(0, DefaultColors.Black, this.GetTemperatureColor(level));
                    this.TemperatureTiles[ix, iy] = tile;
                }
            }*/
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