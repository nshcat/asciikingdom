using System.Linq;
using Engine.Core;
using Game.Maths;

namespace Game.WorldGen
{
    /// <summary>
    /// Base class for a single layer of a world, like temperature, elevation or drainage.
    /// </summary>
    public abstract class Map
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
        public float this[int x, int y]
        {
            get => this.Values[x, y];
            set
            {
                this.Values[x, y] = value;
            }
        }

        /// <summary>
        /// Index operator implementation, which just redirects to the internal
        /// array
        /// </summary>
        public float this[Position position]
        {
            get => this.Values[position.X, position.Y];
            set
            {
                this.Values[position.X, position.Y] = value;
            }
        }
             
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
        /// Normalize the value array <see cref="Values"/>
        /// </summary>
        protected void Normalize()
        {
            this.Normalize(this.Values);
        }
        
        /// <summary>
        /// Normalize the given array of values to [0, 1].
        /// </summary>
        protected void Normalize(float[,] values)
        {
            var max = float.MinValue;
            var min = float.MaxValue;
            
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var value = values[ix, iy];

                    if (value < min)
                        min = value;

                    if (value > max)
                        max = value;
                }
            }
            
            var inputRange = new Maths.FloatRange(min, max);
            var outputRange = new Maths.FloatRange(0.0f, 1.0f);

            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var oldValue = values[ix, iy];
                    values[ix, iy] = MathUtil.Map(oldValue, inputRange, outputRange);
                }
            }
        }

        /// <summary>
        /// Determine the thresholds for a value level based on the fact that <see cref="percentage"/> of
        /// all values have to be lower or equal than it.
        /// </summary>
        protected float CalculateThreshold(float percentage, bool ignoreZeroes = false)
        {
            return this.CalculateThreshold(this.Values, percentage, ignoreZeroes);
        }
        
        /// <summary>
        /// Determine the thresholds for a value level based on the fact that <see cref="percentage"/> of
        /// all values have to be lower or equal than it.
        /// </summary>
        protected float CalculateThreshold(float[,] values, float percentage, bool ignoreZeroes = false)
        {
            var sorted = values.Cast<float>().OrderBy(x => x).ToArray();

            if (ignoreZeroes)
                sorted = sorted.Where(x => x != 0.0f).ToArray();

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