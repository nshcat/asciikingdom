using System.Collections.Generic;
using Engine.Core;
using Game.Data;

namespace Game.WorldGen
{
    /// <summary>
    /// Represents a single river in the world map.
    /// </summary>
    public class River
    {
        /// <summary>
        /// All segments of this river in the form of a set.
        /// </summary>
        /// <remarks>
        /// This data structure allows fast checks whether a certain position is part of this river.
        /// </remarks>
        protected HashSet<Position> Segments { get; }
            = new HashSet<Position>();
        
        /// <summary>
        /// Ordered river path
        /// </summary>
        protected List<Position> Path { get; }
            = new List<Position>();

        /// <summary>
        /// Construct a new river instance with given source location.
        /// </summary>
        public River(Position source)
        {
            this.AddSegment(source);
        }

        /// <summary>
        /// Add a new river path segment.
        /// </summary>
        public void AddSegment(Position position)
        {
            this.Segments.Add(position);
            this.Path.Add(position);
        }

        /// <summary>
        /// Check whether this river contains the given position as a segment.
        /// </summary>
        public bool ContainsSegment(Position position)
        {
            return this.Segments.Contains(position);
        }

        /// <summary>
        /// Convert river path into river tile type values.
        /// </summary>
        public void GenerateTileTypes(RiverTileType[,] tileTypes)
        {
            // Source
            var first = this.Path[0];
            tileTypes[first.X, first.Y] = RiverTileType.Source;

            if (this.Path.Count == 2)
            {
                
            }
            else
            {
                for (var ix = 1; ix < this.Path.Count - 1; ++ix)
                {
                    var previous = this.Path[ix - 1];
                    var current = this.Path[ix];
                    var next = this.Path[ix + 1];
                }    
            }
        }
    }
}