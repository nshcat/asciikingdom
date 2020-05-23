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
        /// The biome map
        /// </summary>
        protected TerrainType[,] Biomes { get; }
        
        /// <summary>
        /// River tile type array
        /// </summary>
        public RiverTileType[,] RiverTileTypes { get; protected set; }

        /// <summary>
        /// How many river starts to generate
        /// </summary>
        protected int IterationCount { get; } = 300;

        /// <summary>
        /// All completed rivers
        /// </summary>
        protected List<River> Rivers { get; }

        /// <summary>
        /// All lake tiles
        /// </summary>
        protected HashSet<(int, int)> Lakes;
        
        /// <summary>
        /// Create new river generator instance
        /// </summary>
        public RiverGenerator(Size dimensions, int seed, HeightMap elevation, TerrainType[,] biomes)
        {
            this.Dimensions = dimensions;
            this.Seed = seed;
            this.Elevation = elevation;
            this.Biomes = biomes;
        }

        /// <summary>
        /// Checks whether given position is a segment of any river that is not the currently created one
        /// </summary>
        protected bool IsInAnyRiver(Position position)
        {
            return this.Rivers.All(river => !river.ContainsSegment(position));
        }
        
        /// <summary>
        /// Generate rivers and replace affected biome tiles with river terrain.
        /// </summary>
        public void GenerateRivers()
        {
            var rng = new Random(this.Seed);
            
            this.RiverTileTypes = new RiverTileType[this.Dimensions.Width, this.Dimensions.Height];
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
                        viableSources.Add((ix, iy));
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

            for (var i = 0; i < this.IterationCount; ++i)
            {
                var position = randomSource();
                var oldPosition = position;

                var river = new River(position);

                while (true)
                {
                    var elevation = this.Elevation[position.Item1, position.Item2];

                    var anyFound = false;
                    var steepestSlope = float.MaxValue;
                    var bestNewPos = (0, 0);
                    var cancelRiver = false;
                    
                    for (var dx = -1; dx <= 1; ++dx)
                    {
                        for (var dy = -1; dy <= 1; ++dy)
                        {
                            var newPos = (position.Item1 + dx, position.Item2 + dy);
                            var newPos_ = new Position(newPos.Item1, newPos.Item2);

                            // Dont go back
                            if(newPos == oldPosition)
                                continue;
                            // If we hit _another_ river that is not the current one, we stop
                            else if (this.IsInAnyRiver(newPos_) || this.Lakes.Contains(newPos))
                            {
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
                                if (this.Biomes[newPos.Item1, newPos.Item2] == TerrainType.Ocean ||
                                    this.Biomes[newPos.Item1, newPos.Item2] == TerrainType.SeaIce)
                                {
                                    cancelRiver = true;
                                }
                                else
                                {
                                    // Calculate slope
                                    var newElevation = this.Elevation[newPos.Item1, newPos.Item2];
                                    var slope = newElevation - elevation;

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
                                cancelRiver = true;
                            }
                        }
                    }

                    if (anyFound && !cancelRiver)
                    {
                        // If the steepest slope is zero or positive, we are in a pit
                        if (steepestSlope < 0.0f)
                        {
                            oldPosition = position;
                            position = bestNewPos;
                            river.AddSegment(new Position(position.Item1, position.Item2));
                        }
                        else
                        {
                            // Create a lake
                            this.Lakes.Add(position);
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                
                this.Rivers.Add(river);
            }
            
            this.UpdateTerrain();
        }
        

        /// <summary>
        /// Update terrain types to respect generated rivers
        /// </summary>
        private void UpdateTerrain()
        {
            foreach (var river in this.Rivers)
            {
                river.GenerateTileTypes(this.RiverTileTypes);
            }
            
            foreach (var (ix, iy) in this.Lakes)
                this.Biomes[ix, iy] = TerrainType.Lake;
        }
    }
}