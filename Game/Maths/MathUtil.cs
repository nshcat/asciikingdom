using System;

namespace Game.Maths
{
    /// <summary>
    /// Collection of useful math functions
    /// </summary>
    public static class MathUtil
    {
        /// <summary>
        /// Map value from input range to given output range
        /// </summary>
        /// <param name="value">The value to map</param>
        /// <param name="inputRange">The range of the input value</param>
        /// <param name="outputRange">The desired output range</param>
        /// <returns>Value mapped into the output range</returns>
        public static float Map(float value, Range inputRange, Range outputRange)
        {
            if (value > inputRange.Maximum)
                return outputRange.Maximum;

            if (value < inputRange.Minimum)
                return outputRange.Minimum;
            
            return (value - inputRange.Minimum) * (outputRange.Maximum - outputRange.Minimum)
                / (inputRange.Maximum - inputRange.Minimum) + outputRange.Minimum;
        }

        /// <summary>
        /// Clamp value to range.
        /// </summary>
        public static int Clamp(int value, int minimum, int maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }
}