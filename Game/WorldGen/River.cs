using System;
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
        /// Represents a combination of a river reference and a segment inside that river.
        /// This class is used to remember into which other river a river ended and joined into.
        /// </summary>
        public class RiverJoin
        {
            /// <summary>
            /// The river that was joined into
            /// </summary>
            public River JoinedRiver { get; }
            
            /// <summary>
            /// In which segment of the river the join happened
            /// </summary>
            public Position JoinLocation { get; }

            /// <summary>
            /// Create a new river join instance
            /// </summary>
            public RiverJoin(River river, Position location)
            {
                this.JoinedRiver = river;
                this.JoinLocation = location;
            }
        }
        
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
        /// Represents a single segment in a river
        /// </summary>
        protected class RiverSegment
        {
            /// <summary>
            /// Size of the river. Is used to differentiate between minor and major rivers.
            /// </summary>
            public int Size { get; set; }
            
            /// <summary>
            /// The position of the segment on the world map.
            /// </summary>
            public Position Position { get; set; }

            /// <summary>
            /// Construct a new river segment instance
            /// </summary>
            public RiverSegment(Position position, int size)
            {
                this.Position = position;
                this.Size = size;
            }

            /// <summary>
            /// Increase the size of this river segment by given river size.
            /// </summary>
            public void IncreaseSize(int otherSize)
            {
                this.Size += otherSize;
            }
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
            RiverTileType.Cross            // 15 (All directions)
        };
        
        /// <summary>
        /// An optional position that marks an invisible end segment of the river. This is used for rivers that
        /// join into an ocean or another river, and makes the ends bend correctly
        /// </summary>
        public Position? EndMarker { get; set; }

        /// <summary>
        /// Whether this river has an end marker
        /// </summary>
        public bool HasEndMarker => this.EndMarker != null;
        
        /// <summary>
        /// Potential river join instance detailing into which river this river joined.
        /// </summary>
        /// <remarks>
        /// This might be null, if this river did not join into another river.
        /// </remarks>
        public RiverJoin EndJoin { get; set; }

        /// <summary>
        /// Whether this river has an end join
        /// </summary>
        public bool HasEndJoin => this.EndJoin != null;
        
        /// <summary>
        /// All segment positions of this river as a set.
        /// </summary>
        /// <remarks>
        /// This data structure allows fast checks whether a certain position is part of this river.
        /// </remarks>
        protected HashSet<Position> SegmentPositions { get; }
            = new HashSet<Position>();
        
        /// <summary>
        /// Ordered river path
        /// </summary>
        protected List<RiverSegment> Path { get; }
            = new List<RiverSegment>();
        
        /// <summary>
        /// Hash map of all joins, which are segments of the river where another river joined (and thus ended) in.
        /// The second position is the last segment of the ended river.
        /// </summary>
        /// <remarks>
        /// This data is used to correctly generate river tile types, especially the connections between joined
        /// rivers.
        /// </remarks>
        protected Dictionary<Position, List<Position>> JoinedRivers { get; }
            = new Dictionary<Position, List<Position>>();

        /// <summary>
        /// Construct a new river instance with given source location.
        /// </summary>
        public River(Position source)
        {
            this.AddSegment(source);
        }

        /// <summary>
        /// Recursively increase the size of this river and its potential join target by given size delta.
        /// </summary>
        public void AdjustSize(Position startLocation, int delta)
        {
            // Try to find the index of the segment corresponding to the start location
            var startIndex = this.Path.FindIndex(x => x.Position == startLocation);
            
            if(startIndex == -1)
                throw new ArgumentException("AdjustSize: Start location is not a river segment");

            for (var i = startIndex; i < this.Path.Count; ++i)
            {
                this.Path[i].Size += delta;
            }
            
            // If we joined into another river, we have to recurse
            if(this.HasEndJoin)
                this.EndJoin.JoinedRiver.AdjustSize(this.EndJoin.JoinLocation, delta);
        }

        /// <summary>
        /// Add a new river path segment.
        /// </summary>
        public void AddSegment(Position position)
        {
            // Initially, all rivers have size 1. Only when other rivers join into them
            // the size of the affected segments is adjusted.
            this.Path.Add(new RiverSegment(position, 1));
            this.SegmentPositions.Add(position);
        }

        /// <summary>
        /// Check whether this river contains the given position as a segment.
        /// </summary>
        public bool ContainsSegment(Position position)
        {
            return this.SegmentPositions.Contains(position);
        }

        /// <summary>
        /// Add a new joined river, which is another river ending into this one.
        /// </summary>
        /// <param name="segment">Segment of this river the other river ended in</param>
        /// <param name="riverEnd">Last segment of the ended river, not intersecting with this river.</param>
        public void AddJoinedRiver(Position segment, Position riverEnd)
        {
            if (!this.JoinedRivers.ContainsKey(segment))
            {
                this.JoinedRivers.Add(segment, new List<Position>{riverEnd});
            }
            else
            {
                this.JoinedRivers[segment].Add(riverEnd);
            }
        }

        /// <summary>
        /// Sets the terrain type to river for all segments of this river
        /// </summary>
        public void SetTerrain(TerrainType[,] terrain)
        {
            foreach (var segment in this.Path)
            {
                var position = segment.Position;
                terrain[position.X, position.Y] = TerrainType.River;
            }
        }

        /// <summary>
        /// Convert river path into river tile type values.
        /// </summary>
        public void GenerateTileTypes(Dictionary<Position, RiverTileInfo> riverTileInfos)
        {
            // Source
            var first = this.Path[0];
            riverTileInfos[first.Position] = new RiverTileInfo(RiverTileType.Source);

            if (this.Path.Count == 1)
                return;
            
            if (this.Path.Count == 2)
            {
                var last = this.Path[1];
                var direction = this.FindDirection(last.Position,  first.Position);

                if (this.HasEndMarker)
                {
                    direction |= this.FindDirection(last.Position, this.EndMarker.Value);
                }

                riverTileInfos[last.Position] = new RiverTileInfo(_riverTileLookupTable[(int) direction], last.Size);
            }
            else
            {
                for (var ix = 1; ix < this.Path.Count - 1; ++ix)
                {
                    var previous = this.Path[ix - 1];
                    var current = this.Path[ix];
                    var next = this.Path[ix + 1];

                    var directionPrev = this.FindDirection(current.Position, previous.Position);
                    var directionNext = this.FindDirection(current.Position, next.Position);
                    var index = (int) (directionPrev | directionNext);
                    
                    // Check if there is a join
                    if (this.JoinedRivers.ContainsKey(current.Position))
                    {
                        var riverEnds = this.JoinedRivers[current.Position];

                        foreach (var end in riverEnds)
                        {
                            var directionJoin = this.FindDirection(current.Position, end);
                            index |= (int) directionJoin;
                        }
                    }
                    
                    riverTileInfos[current.Position] = new RiverTileInfo(
                        _riverTileLookupTable[index],
                        current.Size
                    );
                }
                
                // Last one
                var last = this.Path[^1];
                var penultimate = this.Path[^2];
                var direction = this.FindDirection(last.Position, penultimate.Position);

                // Check if there is a join
                if (this.JoinedRivers.ContainsKey(last.Position))
                {
                    var riverEnds = this.JoinedRivers[last.Position];

                    foreach (var end in riverEnds)
                    {
                        var directionJoin = this.FindDirection(last.Position, end);
                        direction |= directionJoin;
                    }
                }
                
                if (this.HasEndMarker)
                {
                    direction |= this.FindDirection(last.Position, this.EndMarker.Value);
                }

                riverTileInfos[last.Position] = new RiverTileInfo(
                    _riverTileLookupTable[(int) direction],
                    last.Size
                );
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