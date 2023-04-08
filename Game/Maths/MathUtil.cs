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
        public static float Map(float value, FloatRange inputRange, FloatRange outputRange)
        {
            if (value > inputRange.Maximum)
                return outputRange.Maximum;

            if (value < inputRange.Minimum)
                return outputRange.Minimum;
            
            return (value - inputRange.Minimum) * (outputRange.Maximum - outputRange.Minimum)
                / (inputRange.Maximum - inputRange.Minimum) + outputRange.Minimum;
        }

        /// <summary>
        /// Perform a mapping from <paramref name="inputRange"/> to <paramref name="outputRange"/>, with
        /// the maximum value of <paramref name="outputRange"/> being fixed to the center of the <paramref name="inputRange"/>.
        /// Values outside of <paramref name="inputRange"/> are mapped to the lower bound of <paramref name="outputRange"/>.
        /// </summary>
        /// <remarks>
        /// The calculation follows the following graph:
        /// 
        ///      ^
        /// oMax-|     ^
        ///      |    / \
        ///      |   /   \
        /// oMin-|__/     \____
        ///      +------------->
        ///     iMin^      ^iMax
        ///    
        /// </remarks>
        /// <param name="value">The input value to be mapped</param>
        /// <param name="inputRange">The range of valid input values</param>
        /// <param name="outputRange">The target output range to map value to</param>
        /// <returns>Value mapped into <paramref name="outputRange"/>.</returns>
        public static float PeakMap(float value, FloatRange inputRange, FloatRange outputRange)
        {
            var center = (float)((inputRange.Maximum - inputRange.Minimum) / 2.0f) + inputRange.Minimum;

            if(value <= center)
            {
                // Map on rising slope
                return Map(value, new FloatRange(inputRange.Minimum, center), outputRange);
            }
            else
            {
                // Map on falling slope
                return Map(value, new FloatRange(center, inputRange.Maximum), outputRange.Flipped);
            }
        }

        /// <summary>
        /// Clamp value to range.
        /// </summary>
        public static int Clamp(int value, int minimum, int maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }

        /// <summary>
        /// Clamp value to range.
        /// </summary>
        public static float Clamp(float value, float minimum, float maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }
        
        /// <summary>
        /// GLSL smooth step function on [0, 1]
        /// </summary>
        public static float Smoothstep(float x)
        {
            if (x <= 0.0f)
                return 0.0f;

            if (x >= 1.0f)
                return 1.0f;

            return x * x * (3.0f - 2.0f * x);
        }
    }
}