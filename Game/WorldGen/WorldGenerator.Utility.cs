using System.Linq;
using Engine.Core;
using Engine.Graphics;
using Game.Maths;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;

namespace Game.WorldGen
{
    public partial class WorldGenerator
    {
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

        /// <summary>
        /// Check whether value lies inside the given range.
        /// </summary>
        private bool InRange(float x, float min, float max)
        {
            return x >= min && x <= max;
        }

        /// <summary>
        /// Normalize the values contained within given noise map to be inside the range [0, 1].
        /// </summary>
        private void Normalize(NoiseMap noiseMap)
        {
            // Determine range of values in noise map
            var inputRange = new Maths.Range(noiseMap.Data.Min(), noiseMap.Data.Max());
            var outputRange = new Maths.Range(0.0f, 1.0f);

            for (var i = 0; i < noiseMap.Data.Length; ++i)
            {
                var oldValue = noiseMap.Data[i];
                noiseMap.Data[i] = MathUtil.Map(oldValue, inputRange, outputRange);
            }
        }
        
        /// <summary>
        /// Uses given noise module tree to generate a noise map
        /// </summary>
        private NoiseMap GenerateNoiseMap(Size dimensions, Module rootModule, System.Drawing.RectangleF bounds)
        {
            var noiseMap = new NoiseMap();
            var builder = new PlaneNoiseMapBuilder()
            {
                DestNoiseMap = noiseMap,
                SourceModule = rootModule
            };
            builder.SetDestSize(dimensions.Width, dimensions.Height);

            builder.SetBounds(bounds.Left, bounds.Right, bounds.Top, bounds.Bottom);
            builder.Build();

            Normalize(noiseMap);

            return noiseMap;
        }
    }
}