using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Core;
using Game.Data;

namespace Game.WorldGen
{
    /// <summary>
    /// Generates rivers in the game world
    /// </summary>
    public class RiverGenerator
    {
        /// <summary>
        /// Represents the state of the river generator algorithm
        /// </summary>
        private class BacktrackEntry
        {
            /// <summary>
            /// Backtrack marker used to restore previous river state
            /// </summary>
            public int Marker { get; }
            
            /// <summary>
            /// Previous river segment position
            /// </summary>
            public (int, int) PreviousPosition { get; }
            
            /// <summary>
            /// Current river segment position
            /// </summary>
            public (int, int) CurrentPosition { get; }

            /// <summary>
            /// Construct new backtracking entry
            /// </summary>
            public BacktrackEntry(int marker, (int, int) previous, (int, int) current)
            {
                this.Marker = marker;
                this.PreviousPosition = previous;
                this.CurrentPosition = current;
            }
        }
        
        /// <summary>
        /// All positions to check when trying to find the next river segment
        /// </summary>
        private static List<Position> _directions = new List<Position>()
        {
            new Position(1, 0),
            new Position(-1, 0),
            new Position(0, 1),
            new Position(0, -1)
        };
        
        /// <summary>
        /// The map generator seed
        /// </summary>
        protected int Seed { get; }
        
        /// <summary>
        /// World dimensions
        /// </summary>
        protected Size Dimensions { get; }
        
        /// <summary>
        /// The elevation component of the map
        /// </summary>
        protected HeightMap Elevation { get; }
        
        /// <summary>
        /// The temperature component of the map
        /// </summary>
        protected TemperatureMap Temperature { get; }

        /// <summary>
        /// The minium distance between two river sources, in tiles.
        /// </summary>
        protected float MinimumSourceDistance { get; } = 8.0f;
        
        /// <summary>
        /// Extra river tile data
        /// </summary>
        public Dictionary<Position, RiverTileInfo> RiverTileInfo { get; protected set; }

        /// <summary>
        /// How many river starts to generate
        /// </summary>
        protected int IterationCount { get; } = 30;

        /// <summary>
        /// The chance of a river randomly winding
        /// </summary>
        protected double RiverWindChance { get; } = 0.20;//0.05;

        /// <summary>
        /// All completed rivers
        /// </summary>
        protected List<River> Rivers { get; set;  }

        /// <summary>
        /// All lake tiles
        /// </summary>
        protected HashSet<(int, int)> Lakes;
        
        /// <summary>
        /// Create new river generator instance
        /// </summary>
        public RiverGenerator(Size dimensions, int seed, HeightMap elevation, TemperatureMap temperature)
        {
            this.Dimensions = dimensions;
            this.Seed = seed;
            this.Elevation = elevation;
            this.Temperature = temperature;
        }

        /// <summary>
        /// Checks whether given position is a segment of any river that is not the currently created one
        /// </summary>
        public bool IsInAnyRiver(Position position)
        {
            return !this.Rivers.All(river => !river.ContainsSegment(position));
        }

        /// <summary>
        /// Retriever the river the current river will join into
        /// </summary>
        protected River GetJoinedIntoRiver(Position position)
        {
            return this.Rivers.First(river => river.ContainsSegment(position));
        }
        
        /// <summary>
        /// Check if randomly walking into the direction specified by <see cref="current"/> would cause a lake to form.
        /// </summary>
        /// <remarks>
        /// Sometimes, when adding random wiggle to rivers, we get into a situation where picking a random slope direction
        /// will cause us to be in a position where the only downwards slope is the tile we just came from, thus forcing
        /// the river generator to place a lake and terminate the river. This method detects this case by checking whether
        /// the position we came from is the only legal move.
        /// </remarks>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        protected bool WouldGenerateRiver(Position previous, Position current)
        {
            var legalMoves = new List<Position>();
            
            foreach (var direction in _directions)
            {
                var neighbour = direction + current;
                
                if (neighbour == previous)
                    continue;

                if (neighbour.X >= 0 && neighbour.X < this.Dimensions.Width &&
                    neighbour.Y >= 0 && neighbour.Y < this.Dimensions.Height)
                {
                    // If its an ocean or sea ice, we return false since the algorithm always picks these
                    var elevation = this.Elevation[neighbour.X, neighbour.Y];
                    if (elevation <= this.Elevation.SeaThreshold)
                        return false;
                    
                    // If its another river, we also return false since the algorithm picks it.
                    if (this.IsInAnyRiver(neighbour))
                        return false;
                    
                    var oldElevation = this.Elevation[current];
                    var newElevation = this.Elevation[neighbour];
                    var slope = newElevation - oldElevation;
                    
                    if(slope < 0.0f)
                        legalMoves.Add(neighbour);
                }
                else // Would just run out of the map
                {
                    return false;
                }
            }

            return legalMoves.Count == 0;
        }

        /// <summary>
        /// Generate rivers and replace affected biome tiles with river terrain.
        /// </summary>
        public void GenerateRivers()
        {
            var rng = new Random(this.Seed);
            var windingRng = new Random(this.Seed + 4215);
            
            this.RiverTileInfo = new Dictionary<Position, RiverTileInfo>();
            this.Rivers = new List<River>();
            this.Lakes = new HashSet<(int, int)>();
            
            // Find all points over mountain threshold
            var viableSources = new List<(int, int)>();

            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    if (this.Elevation[ix, iy] <= this.Elevation.LowMountainThreshold
                    && this.Elevation[ix, iy] >= this.Elevation.LandThreshold)
                    {
                        var temperature = this.Temperature.TemperatureLevels[ix, iy];

                        if (temperature != TemperatureLevel.Coldest 
                            && temperature != TemperatureLevel.Colder
                        )
                        {
                            viableSources.Add((ix, iy));
                        }
                    }
                }
            }

            // Function to retrieve a random river source
            Func<(int, int)> randomSource = () =>
            {
                var index = rng.Next(viableSources.Count);
                var source = viableSources[index];
                viableSources.RemoveAt(index);
                return source;
            };

            int riverId = 0;
            for (var i = 0; i < this.IterationCount; ++i)
            {
                var position = randomSource();
                
                // Remove all other sources that are too close or on an existing river. Since we do this for every river we add, we only
                // have to check the distance to the newly picked source.
                viableSources = viableSources.Where(pos =>
                {
                    var lhs = new Position(pos.Item1, pos.Item2);
                    var rhs = new Position(position.Item1, position.Item2);
                    var distance = Position.GetDistance(lhs, rhs);

                    return distance >= this.MinimumSourceDistance && !this.IsInAnyRiver(lhs);
                }).ToList();

                // Reject river source if it is part of an already existing river
                if(this.IsInAnyRiver(new Position(position.Item1, position.Item2)))
                    continue;

                var oldPosition = position;

                var river = new River(new Position(position.Item1, position.Item2));
                var backtrackEntries = new Stack<BacktrackEntry>();
                var wasBacktracked = false;
                var outOfMap = false;

                while (true)
                {
                    var elevation = this.Elevation[position.Item1, position.Item2];

                    var anyFound = false;
                    var steepestSlope = float.MaxValue;
                    var bestNewPos = (0, 0);
                    var cancelRiver = false;
                    var endedInRiver = false;
                    var allowedPositions = new List<Position>(); // All positions that would be legal river segments
                    
                    foreach(var direction in _directions)
                    {
                        var newPos = (position.Item1 + direction.X, position.Item2 + direction.Y);
                        var newPos_ = new Position(newPos.Item1, newPos.Item2);

                        // Dont go back
                        if(newPos == oldPosition)
                            continue;
                        // If we hit _another_ river that is not the current one, we stop
                        else if (this.IsInAnyRiver(newPos_))
                        {
                            cancelRiver = true;
                            
                            // Notify the other river that we ended into it
                            var otherRiver = this.GetJoinedIntoRiver(newPos_);
                            otherRiver?.AddJoinedRiver(newPos_, new Position(position.Item1, position.Item2));
                            
                            // Since this river is new, it is of size 1
                            otherRiver?.AdjustSize(newPos_, 1);
                            river.EndMarker = newPos_;

                            if (otherRiver != null)
                            {
                                river.EndJoin = new River.RiverJoin(otherRiver, newPos_);
                            }

                            endedInRiver = true;
                            
                            break;
                        }
                        else if (this.Lakes.Contains(newPos))
                        {
                            // Otherwise we can't do anything anymore
                            cancelRiver = true;
                            break;
                        }
                        // We would run into ourselves. ignore.
                        else if (river.ContainsSegment(newPos_))
                        {
                            continue;
                        }
                        else if(newPos.Item1 >= 0 && newPos.Item1 < this.Dimensions.Width &&
                                    newPos.Item2 >= 0 && newPos.Item2 < this.Dimensions.Height)
                        {
                            // End on ocean
                            if (this.Elevation[newPos.Item1, newPos.Item2] <= this.Elevation.SeaThreshold)
                            {
                                river.EndMarker = newPos_;
                                cancelRiver = true;
                            }
                            else
                            {
                                // Calculate slope
                                var newElevation = this.Elevation[newPos.Item1, newPos.Item2];
                                var slope = newElevation - elevation;
                                allowedPositions.Add(newPos_); // TODO this causes rivers to flow upwards!
                                // => maybe try to fix this by only collecting positions that have negative slope

                                if (slope < steepestSlope)
                                {
                                    steepestSlope = slope;
                                    bestNewPos = newPos;
                                    anyFound = true;
                                }
                            }
                        }
                        else // Cancel rivers that run outside of the map
                        {
                            outOfMap = true;
                            cancelRiver = true;
                        }
                    }

                    if (anyFound && !cancelRiver)
                    {
                        // If the steepest slope is zero or positive, we are in a pit
                        if (steepestSlope < 0.0f)
                        {
                            var nextPosition = bestNewPos;

                            // Do not do a random wiggle if we had a problem with the choice earlier
                            if (!wasBacktracked)
                            {
                                // Have a slight chance to just wiggle
                                if (windingRng.NextDouble() <= this.RiverWindChance)
                                {
                                    var legalPositions = allowedPositions
                                        .Where(pos =>
                                            !this.WouldGenerateRiver(new Position(position.Item1, position.Item2), pos))
                                        .Where(pos => pos != new Position(bestNewPos.Item1, bestNewPos.Item2))
                                        .ToList();

                                    if (legalPositions.Count > 0)
                                    {
                                        var next = legalPositions[windingRng.Next(legalPositions.Count)];
                                        nextPosition = (next.X, next.Y);
                                        
                                        // Save state for backtracking
                                        backtrackEntries.Push(
                                            new BacktrackEntry(
                                                river.GetBacktrackMarker(),
                                                oldPosition,
                                                position
                                                )
                                            );
                                    }
                                }
                            }
                            else
                            {
                                wasBacktracked = false;
                            }

                            oldPosition = position;
                            position = nextPosition;
                            river.AddSegment(new Position(position.Item1, position.Item2));
                        }
                        else if(!outOfMap)
                        {
                            // Did we perform any random choices in the past?
                            if (backtrackEntries.Count > 0)
                            {
                                // Backtrack
                                var entry = backtrackEntries.Pop();
                                river.Backtrack(entry.Marker);
                                oldPosition = entry.PreviousPosition;
                                position = entry.CurrentPosition;
                                wasBacktracked = true;
                            }
                            else
                            {
                                // Create a lake
                                this.Lakes.Add(position);
                                break;
                            }
                        }
                    }
                    else if(!wasBacktracked)
                    {
                        // Rare case: No tile was available! This should only happen if we applied some random winding.
                        if (!anyFound && !endedInRiver && backtrackEntries.Count > 0)
                        {
                            // Backtrack
                            var entry = backtrackEntries.Pop();
                            river.Backtrack(entry.Marker);
                            oldPosition = entry.PreviousPosition;
                            position = entry.CurrentPosition;
                            wasBacktracked = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                
                this.Rivers.Add(river);
                riverId++;
            }
        }
        

        /// <summary>
        /// Update terrain types to respect generated rivers
        /// </summary>
        public void UpdateTerrain(TerrainType[,] biomes)
        {
            foreach (var river in this.Rivers)
            {
                river.GenerateTileTypes(this.RiverTileInfo);
                river.SetTerrain(biomes);
            }
            
            foreach (var (ix, iy) in this.Lakes)
                biomes[ix, iy] = TerrainType.Lake;
        }
    }
}