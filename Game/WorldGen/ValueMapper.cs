using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Game.Maths;

namespace Game.WorldGen
{
    /// <summary>
    /// A class linearly mapping intervals of values to new intervals.
    /// </summary>
    /// <remarks>
    /// This class is used in the world generation progress, where thresholds for different terrain intervals
    /// are determined at runtime.
    ///
    /// If a mapper contains the entries (0.35, 0.5) and (0.5, 0.85), then this describes the following intervals:
    ///  - [0, 0.35] which gets mapped to [0, 0.5]
    ///  - [0.35, 0.5] which gets mapped to [0.5, 0.85]
    ///  - [0.5, 1.0] which gets mapped to [0.85, 1]
    ///
    /// The first number in each entry is the upper bound in the source range, and the second number is
    /// the upper bound in the destination range.
    /// </remarks>
    public class ValueMapper : IEnumerable<(float, float)>
    {
        /// <summary>
        /// All entries in this value mapper
        /// </summary>
        protected List<(float SourceEnd, float DestinationEnd)> Entries { get; set; }

        /// <summary>
        /// Create a new value mapper instance with given entries.
        /// </summary>
        public ValueMapper(params (float SourceEnd, float DestinationEnd)[] entries)
        {
            this.Entries = new List<(float SourceEnd, float DestinationEnd)>(entries);
        }

        public ValueMapper()
        {
            this.Entries = new List<(float SourceEnd, float DestinationEnd)>();
        }

        public void Add(float sourceEnd, float destinationEnd)
        {
            this.Entries.Add((sourceEnd, destinationEnd));
        }

        /// <summary>
        /// Map given value according to the intervals defined by the stored entries.
        /// </summary>
        public float Map(float value)
        {
            if (this.Entries.Count <= 0)
                return 0.0f;

            var sourceEnd = 0.0f;
            var sourceLength = 0.0f;
            var destinationEnd = 0.0f;
            var destinationLength = 0.0f;
            for (var i = 0; i < this.Entries.Count; ++i)
            {
                var entry = this.Entries[i];
                var sourceBegin = sourceEnd;
                sourceEnd = entry.SourceEnd;

                var destinationBegin = destinationEnd;
                destinationEnd = entry.DestinationEnd;
                

                if (value <= sourceEnd)
                {
                    // Map it into the range
                    var sourceRange = new FloatRange(sourceBegin, sourceEnd);
                    var destinationRange = new FloatRange(destinationBegin, destinationEnd);
                    return MathUtil.Map(value, sourceRange, destinationRange);
                }
            }
            
            // Value is in last range
            var lastSourceRange = new FloatRange(sourceEnd, 1.0f);
            var lastDestinationRange = new FloatRange(destinationEnd, 1.0f);

            return MathUtil.Map(value, lastSourceRange, lastDestinationRange);
        }
        
        public IEnumerator<(float, float)> GetEnumerator()
        {
            return this.Entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}