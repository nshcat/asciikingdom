using System;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Engine.Core;
using OpenToolkit.Graphics.OpenGL;

namespace Game.WorldGen
{
    /// <summary>
    /// Represents a single cloud used to generate the rainfall map
    /// </summary>
    public class Cloud
    {
        /// <summary>
        /// The X coordinate of the clouds center
        /// </summary>
        public float X { get; set; }
        
        /// <summary>
        /// The Y coordinate of the clouds center
        /// </summary>
        public float Y { get; set; }
        
        /// <summary>
        /// The dimensions of a cloud, in tiles
        /// </summary>
        public Size Dimensions { get; protected set; } = new Size(7, 7);
        
        /// <summary>
        /// The elevation layer of the world
        /// </summary>
        protected HeightMap Elevation { get; set; }

        /// <summary>
        /// The temperature below which cloud begins to form rain droplets
        /// </summary>
        protected float DewPoint { get; set; } = -1000.0f;
        
        /// <summary>
        /// The rainfall layer of the world
        /// </summary>
        /// <remarks>
        /// This is being modified by the clouds, by dropping rain on the map according to
        /// elevation data
        /// </remarks>
        protected RainfallMap Rainfall { get; set; }

        /// <summary>
        /// Create a new cloud instance.
        /// </summary>
        public Cloud(HeightMap elevation, RainfallMap rainfall, Position position)
        {
            this.Elevation = elevation;
            this.Rainfall = rainfall;
            
            this.X = position.X;
            this.Y = position.Y;
            var centerPos = this.GetAbsoluteCenter();
            var height = this.Elevation[centerPos];
            var temp = 1.0f - height;
            var seaTemp = 1.0f - this.Elevation.SeaThreshold;

            this.DewPoint = (height <= this.Elevation.SeaThreshold)
                ? seaTemp
                : temp - (temp / 2.0f);
        }

        /// <summary>
        /// Move cloud one step into given direction
        /// </summary>
        public void Move(Vector2 direction)
        {
            this.X += direction.X;
            this.Y += direction.Y;
        }

        /// <summary>
        /// Drop rain at the current cloud position
        /// </summary>
        public void DropRain(float weight)
        {
            var centerPos = this.GetAbsoluteCenter();
            var temperature = 1.0f - this.Elevation[centerPos];
            var seaTemp = 1.0f - this.Elevation.SeaThreshold;
            var rainAmount = this.CalculateRainAmount(temperature, seaTemp);
            var width = Math.Max((float) this.Dimensions.Width, (float) this.Dimensions.Height);

            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var pos = this.GetAbsolutePosition(new Position(ix, iy));

                    if (pos.IsInBounds(this.Rainfall.Dimensions))
                    {
                        var distance = Position.GetDistance(centerPos, pos);
                        var rain = rainAmount * weight * ((width / 2.0f) - distance);
                        rain = Math.Max(0.0f, rain);

                        this.Rainfall[pos] += rain;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the rain amount to drop based on given temperature and sea temperature
        /// </summary>
        protected float CalculateRainAmount(float temperature, float seaTemperature)
        {
            var wasHigher = false;
            var rainAmount = 0.0f;

            if (temperature > seaTemperature)
            {
                wasHigher = true;
                temperature = seaTemperature;
            }
            else
            {
                rainAmount = this.DewPoint - temperature;
            }

            if (rainAmount > 0.0f)
            {
                this.DewPoint = temperature;
            }
            else
            {
                rainAmount = 0.0f;
            }

            rainAmount += 0.04f * this.DewPoint;

            if (wasHigher)
            {
                this.DewPoint += (seaTemperature - this.DewPoint) * 0.075f;
            }

            return rainAmount;
        }

        /// <summary>
        /// Retrieve absolute map position of given relative position inside the cloud.
        /// </summary>
        /// <remarks>
        /// This method might return map coordinates that are outside of the map bounds.
        /// The calling code has to make sure to clamp them.
        /// </remarks>
        protected Position GetAbsolutePosition(Position position)
        {
            var halfSize = (Position)(this.Dimensions * 0.5f);
            return new Position((int) this.X, (int) this.Y) - halfSize + position;
        }

        /// <summary>
        /// Get absolute map position of cloud center
        /// </summary>
        protected Position GetAbsoluteCenter()
        {
            var halfSize = (Position)(this.Dimensions * 0.5f);
            return this.GetAbsolutePosition(halfSize);
        }
    }
}