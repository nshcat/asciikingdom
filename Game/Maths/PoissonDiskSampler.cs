using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Core;
using Game.Utility;

namespace Game.Maths
{
    /// <summary>
    /// Implements the poisson disk sampling algorithm for pseudo-randomly picking points on a plane with even spacing
    /// </summary>
    public class PoissonDiskSampler
    {
        /// <summary>
        /// Dimensions of the plane, in cells
        /// </summary>
        protected Size Dimensions { get; set; }
        
        /// <summary>
        /// Minimum allowed distance between sampled points
        /// </summary>
        protected float MinimumDistance { get; set; }

        /// <summary>
        /// Maximum number of tries before a point is given up
        /// </summary>
        protected int MaximumTries { get; set; } = 30;

        /// <summary>
        /// Size of each grid cell
        /// </summary>
        protected float CellSize { get; set; }
        
        /// <summary>
        /// The dimensions of the internal grid
        /// </summary>
        protected Size GridDimensions { get; set; }

        protected class Grid
        {
            /// <summary>
            /// Dimensions of the grid
            /// </summary>
            protected Size Dimensions { get; set; }

            /// <summary>
            /// Grid entries, can be empty
            /// </summary>
            protected Vector2?[,] Entries { get; set; }

            /// <summary>
            /// Initialize new grid instance
            /// </summary>
            public Grid(Size dimensions)
            {
                this.Dimensions = dimensions;
            }

            /// <summary>
            /// Set given grid cell to contain given point.
            /// </summary>
            public void SetPoint(Position gridPosition, Vector2 point)
            {
                this.Entries[gridPosition.X, gridPosition.Y] = point;
            }

            /// <summary>
            /// Check if given grid cell contains a point.
            /// </summary>
            public bool HasPoint(Position gridPosition)
            {
                return this.Entries[gridPosition.X, gridPosition.Y].HasValue;
            }

            /// <summary>
            /// Retrieve point stored in given grid cell, if any.
            /// </summary>
            public Vector2 GetPoint(Position gridPosition)
            {
                if(!this.HasPoint(gridPosition))
                    throw new InvalidOperationException("Specified grid cell does not contain a point");

                return this.Entries[gridPosition.X, gridPosition.Y].Value;
            }
        }

        /// <summary>
        /// Create new possion disk sampler instance
        /// </summary>
        public PoissonDiskSampler(Size dimensions, float minimumDistance)
        {
            Dimensions = dimensions;
            MinimumDistance = minimumDistance;
            this.CellSize = (float)Math.Floor(this.MinimumDistance / Math.Sqrt(2.0f));

            var gridWidth = Math.Ceiling((double) this.Dimensions.Width / this.CellSize);
            var gridHeight = Math.Ceiling((double) this.Dimensions.Height / this.CellSize);
            this.GridDimensions = new Size((int)gridWidth, (int)gridHeight);
        }

        /// <summary>
        /// Sample random points on the plane.
        /// </summary>
        public List<Position> Sample(Random rng)
        {
            var grid = new Grid(this.GridDimensions);
            var points = new List<Vector2>();
            var active = new List<Vector2>();
            
            // Choose initial point at random
            var initialX = rng.NextDouble() * this.Dimensions.Width;
            var initialY = rng.NextDouble() * this.Dimensions.Height;
            var initial = new Vector2((float)initialX, (float)initialY);
            
            this.InsertPoint(grid, initial);
            points.Add(initial);
            active.Add(initial);

            while (active.Count > 0)
            {
                // Select and remove random point from active list
                var index = rng.Next(active.Count);
                var point = active[index];
                active.RemoveAt(index);

                for (var tries = 0; tries < this.MaximumTries; ++tries)
                {
                    // Pick random angle
                    var theta = (float)(2.0 * Math.PI * rng.NextDouble());
                    
                    // Pick random radius
                    var radius = (float)rng.NextDouble(this.MinimumDistance, this.MinimumDistance * 2.0f);
                    
                    var newPoint = new Vector2(
                        point.X + radius * (float)Math.Cos(theta),
                        point.Y + radius * (float)Math.Sin(theta)
                    );
                    
                    if(!this.IsValidPoint(grid, newPoint))
                        continue;
                    
                    // Point is valid
                    points.Add(newPoint);
                    this.InsertPoint(grid, newPoint);
                    active.Add(newPoint);
                    active.Add(point);
                    break;
                }
            }

            return points.Select(v => new Position((int) v.X, (int) v.Y)).ToList();
        }

        /// <summary>
        /// Insert given point into the corresponding grid cell
        /// </summary>
        protected void InsertPoint(Grid grid, Vector2 point)
        {
            var ix = point.X / this.CellSize;
            var iy = point.Y / this.CellSize;
            grid.SetPoint(new Position((int)ix, (int)iy), point);
        }

        /// <summary>
        /// Check whether given point is valid in the context of the algorithm
        /// </summary>
        protected bool IsValidPoint(Grid grid, Vector2 point)
        {
            if (point.X < 0.0 || point.X >= this.Dimensions.Width ||
                point.Y < 0.0 || point.Y >= this.Dimensions.Height)
                return false;
            
            var ix = (int)(point.X / this.CellSize);
            var iy = (int)(point.Y / this.CellSize);

            var i0 = Math.Max(ix - 1, 0);
            var i1 = Math.Min(ix + 1, this.GridDimensions.Width - 1);
            var j0 = Math.Max(iy - 1, 0);
            var j1 = Math.Min(iy + 1, this.GridDimensions.Height - 1);

            for (var iix = i0; iix <= i1; ++iix)
            {
                for (var iiy = j0; iiy <= j1; ++iiy)
                {
                    var gridPosition = new Position(iix, iiy);
                    if (grid.HasPoint(gridPosition))
                    {
                        var otherPoint = grid.GetPoint(gridPosition);
                        var distance = (otherPoint - point).Length();

                        if (distance < this.MinimumDistance)
                            return false;
                    }
                }
            }

            return true;
        }
    }
}