using System;
using System.Collections.Generic;
using System.Text;
using Game.Maths;

namespace Game.Core
{
    /// <summary>
    /// Class that manages crop fertility data and helper functions
    /// </summary>
    public class CropFertility
    {
        /// <summary>
        /// Drainage requirement range
        /// </summary>
        public FloatRange Drainage { get; set; }

        /// <summary>
        /// Rainfall requirement range
        /// </summary>
        public FloatRange Rainfall { get; set; }

        /// <summary>
        /// Temperature requirement range
        /// </summary>
        public FloatRange Temperature { get; set; }

        /// <summary>
        /// Weight for drainage contribution to overall fertility
        /// </summary>
        public float DrainageWeight { get; set; }

        /// <summary>
        /// Weight for rainfall contribution to overall fertility
        /// </summary>
        public float RainfallWeight { get; set; }

        /// <summary>
        /// Weight for temperature contribution to overall fertility
        /// </summary>
        public float TemperatureWeight { get; set; }

        /// <summary>
        /// Calculate total fertility factor in [0, 1] for a crop with these
        /// fertility factor
        /// </summary>
        public float CalculateFertilityFactor(float temperature, float drainage, float rainfall)
        {
            var unitRange = FloatRange.UnitRange;
            float drainageFactor = MathUtil.Map(drainage, this.Drainage, unitRange);
            float rainfallFactor = MathUtil.Map(rainfall, this.Rainfall, unitRange);
            float temperatureFactor = MathUtil.Map(temperature, this.Temperature, unitRange);

            return MathUtil.Clamp((this.DrainageWeight * drainageFactor)
                + (this.RainfallWeight * rainfallFactor)
                + (this.TemperatureWeight * temperatureFactor), 0.0f, 1.0f);
        }
    }
}
