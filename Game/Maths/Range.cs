namespace Game.Maths
{
    /// <summary>
    /// Represents a floating point range.
    /// </summary>
    public struct Range
    {
        /// <summary>
        /// The lower bound (inclusive)
        /// </summary>
        public float Minimum { get; }
        
        /// <summary>
        /// The upper bound (inclusive)
        /// </summary>
        public float Maximum { get; }

        /// <summary>
        /// Construct a new range instance from given bounds.
        /// </summary>
        public Range(float minimum, float maximum)
        {
            this.Maximum = maximum;
            this.Minimum = minimum;
        }
        
        /// <summary>
        /// Determine whether given value lies within the range defined by this instance.
        /// </summary>
        public bool IsInside(float value) => (value >= this.Minimum) && (value <= this.Maximum);
    }
}