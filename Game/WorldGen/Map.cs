using System.Linq;
using Engine.Core;
using Game.Maths;

namespace Game.WorldGen
{
    /// <summary>
    /// Base class for a single layer of a world, like temperature, elevation or drainage.
    /// </summary>
    internal abstract class Map
    {
        /// <summary>
        /// Dimensions of the layer, in tiles
        /// </summary>
        public Size Dimensions { get; protected set; }
        
        /// <summary>
        /// World parameters to use
        /// </summary>
        public WorldParameters Parameters { get; protected set; }
        
        /// <summary>
        /// The underlying, raw data
        /// </summary>
        public float[,] Values { get; protected set; }
        
        /// <summary>
        /// The seed to use for generation
        /// </summary>
        public int Seed { get; protected set; }

        /// <summary>
        /// Index operator implementation, which just redirects to the internal
        /// array
        /// </summary>
        public float this[int x, int y] => this.Values[x, y];
        
        /// <summary>
        /// Create a new, empty world layer instance.
        /// </summary>
        public Map(Size dimensions, int seed, WorldParameters parameters)
        {
            this.Parameters = parameters;
            this.Seed = seed;
            this.Dimensions = dimensions;
            this.Values = new float[dimensions.Width, dimensions.Height];
        }

        /// <summary>
        /// Normalize the raw value array to [0, 1].
        /// </summary>
        protected void Normalize()
        {
            var max = float.MinValue;
            var min = float.MaxValue;
            
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var value = this.Values[ix, iy];

                    if (value < min)
                        min = value;

                    if (value > max)
                        max = value;
                }
            }
            
            var inputRange = new Maths.Range(min, max);
            var outputRange = new Maths.Range(0.0f, 1.0f);

            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var oldValue = this.Values[ix, iy];
                    this.Values[ix, iy] = MathUtil.Map(oldValue, inputRange, outputRange);
                }
            }
        }
        
        /// <summary>
        /// Determine the thresholds for a value level based on the fact that <see cref="percentage"/> of
        /// all values have to be lower or equal than it.
        /// </summary>
        protected float CalculateThreshold(float percentage)
        {
            var sorted = this.Values.Cast<float>().OrderBy(x => x).ToArray();

            if (percentage >= 1.0f)
                return 1.01f;

            if (percentage <= 0.0f)
                return -0.01f;

            if (sorted.Length == 0)
                return 0.0f;
            else
            {
                var index = (int) ((float) sorted.Length * percentage);

                return sorted[index];
            }
        }
    }
}