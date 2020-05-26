using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Engine.Core;
using Game.Data;
using OpenToolkit.Graphics.OpenGL4;

namespace Game.WorldGen
{
    /// <summary>
    /// Represents a single river in the world map.
    /// </summary>
    public class River
    {
        /// <summary>
        /// Flag field of possible directions another river tile connects on
        /// </summary>
        [Flags]
        private enum Direction
        {
            None = 0,
            North = 1,
            East = 2,
            South = 4,
            West = 8
        }

        /// <summary>
        /// Lookup table to determine river tile type based on connections
        /// </summary>
        private static RiverTileType[] _riverTileLookupTable =
        {
            RiverTileType.None,            // 0 (None)
            RiverTileType.Vertical,        // 1 (North)
            RiverTileType.Horizontal,      // 2 (East)
            RiverTileType.NorthEast,       // 3 (North and East)
            RiverTileType.Vertical,        // 4 (South)
            RiverTileType.Vertical,        // 5 (South and North)
            RiverTileType.SouthEast,       // 6 (South and East)
            RiverTileType.VerticalEast,    // 7 (South, North and East)
            RiverTileType.Horizontal,      // 8 (West)
            RiverTileType.NorthWest,       // 9 (North and West)
            RiverTileType.Horizontal,      // 10 (East and West)
            RiverTileType.HorizontalNorth, // 11 (North, East and West)
            RiverTileType.SouthWest,       // 12 (South and West)
            RiverTileType.VerticalWest,    // 13 (North, South and West)
            RiverTileType.HorizontalSouth, // 14 (East, South, and West)
            RiverTileType.Source           // 15 (All directions)
        };
        
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
        /// Hash map of all joins, which are segments of the river where another river joined (and thus ended) in.
        /// The second position is the last segment of the ended river.
        /// </summary>
        protected Dictionary<Position, List<Position>> Joins { get; }
            = new Dictionary<Position, List<Position>>();

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
        /// Add a new river join, which is another river ending into this one.
        /// </summary>
        /// <param name="segment">Segment of this river the other river ended in</param>
        /// <param name="riverEnd">Last segment of the ended river, not intersecting with this river.</param>
        public void AddJoin(Position segment, Position riverEnd)
        {
            if (!this.Joins.ContainsKey(segment))
            {
                this.Joins.Add(segment, new List<Position>{riverEnd});
            }
            else
            {
                this.Joins[segment].Add(riverEnd);
            }
        }

        /// <summary>
        /// Sets the terrain type to river for all segments of this river
        /// </summary>
        public void SetTerrain(TerrainType[,] terrain)
        {
            foreach (var segment in this.Path)
            {
                terrain[segment.X, segment.Y] = TerrainType.River;
            }
        }

        /// <summary>
        /// Convert river path into river tile type values.
        /// </summary>
        public void GenerateTileTypes(RiverTileType[,] tileTypes)
        {
            // Source
            var first = this.Path[0];
            tileTypes[first.X, first.Y] = RiverTileType.Source;

            if (this.Path.Count <= 2)
            {
                
            }
            else
            {
                for (var ix = 1; ix < this.Path.Count - 1; ++ix)
                {
                    var previous = this.Path[ix - 1];
                    var current = this.Path[ix];
                    var next = this.Path[ix + 1];

                    var directionPrev = this.FindDirection(current, previous);
                    var directionNext = this.FindDirection(current, next);
                    var index = (int) (directionPrev | directionNext);
                    
                    // Check if there is a join
                    if (this.Joins.ContainsKey(current))
                    {
                        var riverEnds = this.Joins[current];

                        foreach (var end in riverEnds)
                        {
                            var directionJoin = this.FindDirection(current, end);
                            index |= (int) directionJoin;
                        }
                    }
                    
                    tileTypes[current.X, current.Y] = _riverTileLookupTable[index];
                }
                
                // Last one
                var last = this.Path[^1];
                var penultimate = this.Path[^2];
                var direction = this.FindDirection(last, penultimate);
                tileTypes[last.X, last.Y] = _riverTileLookupTable[(int) direction];
            }
        }

        /// <summary>
        /// Find in which direction the other position lies in
        /// </summary>
        private Direction FindDirection(Position current, Position other)
        {
            var delta = other - current;

            if (delta.X == 1)
                return Direction.East;
            else if (delta.X == -1)
                return Direction.West;
            else if (delta.Y == -1)
                return Direction.North;
            else if (delta.Y == 1)
                return Direction.South;
            else
                throw new ArgumentException("Invalid positions");
        }
    }
}