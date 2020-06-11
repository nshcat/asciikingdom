using System;
using System.Collections;
using System.Collections.Generic;
using Engine.Core;

namespace Game.Maths
{
    /// <summary>
    /// Represents a circle on the world map with a given radius. Allows enumerations
    /// of all point inside of it.
    /// </summary>
    public class Circle : IEnumerable<Position>
    {
        /// <summary>
        /// Collection of all points inside the circle
        /// </summary>
        public List<Position> Points { get; }

        /// <summary>
        /// The radius of the circle, in tiles
        /// </summary>
        public int Radius { get; }
        
        /// <summary>
        /// The center of the circle
        /// </summary>
        public Position Center { get; }

        /// <summary>
        /// Create an new circle instance with given radius
        /// </summary>
        public Circle(Position center, int radius)
        {
            this.Points = new List<Position>();
            this.Radius = radius;
            this.Center = center;

            this.GenerateCircle();
        }

        /// <summary>
        /// Checks whether given point is inside this circle
        /// </summary>
        public bool ContainsPoint(Position point)
        {
            return this.Points.Contains(point);
        }

        /// <summary>
        /// Determine all points inside the circle
        /// </summary>
        private void GenerateCircle()
        {
            var radiusSquared = this.Radius * this.Radius;
            
            // Iterate over x-axis and generate corresponding y values using the circle equation
            for(var ix = -this.Radius; ix <= this.Radius; ++ix)
            {
                // Calculate height based on circle formula
                var h = (int)Math.Sqrt(radiusSquared - (ix*ix));

                // "Draw" vertical line to fill this segment of the circle
                for(int iy = -h; iy <= h; ++iy)
                    this.Points.Add(new Position(ix + this.Center.X, iy + this.Center.Y));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Position> GetEnumerator()
        {
            return this.Points.GetEnumerator();
        }
    }
}