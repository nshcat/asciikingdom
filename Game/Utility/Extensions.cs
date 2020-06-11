using System;
using System.Collections.Generic;

namespace Game.Utility
{
    /// <summary>
    /// Contains various extensions methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Randomly shuffle given list
        /// </summary>
        public static void Shuffle<T>(this IList<T> list, Random rng)  
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
        
        /// <summary>
        /// Retrieve next random double in given range
        /// </summary>
        public static double NextDouble(this Random random, double minValue, double maxValue)
        {
            double sample = random.NextDouble();
            return (maxValue * sample) + (minValue * (1d - sample));
        }
    }
}