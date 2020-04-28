using System;

// TODO: IsInBounds(Rectangle)
namespace Engine.Core
{
    /// <summary>
    /// Represents an integral position in the two-dimensional plane.
    /// </summary>
    public struct Position : IEquatable<Position>
    {
        /// <summary>
        /// The x-coordinate of the position.
        /// </summary>
        public int X { get; set; }
        
        /// <summary>
        /// The y-coordinate of the position.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Whether this position represents the origin (both coordinates zero)
        /// </summary>
        public bool IsOrigin => this.X == 0 && this.Y == 0;

        /// <summary>
        /// The origin position, with both coordinates set to zero.
        /// </summary>
        public Position Origin => new Position(0, 0);

        /// <summary>
        /// Construct a position with given coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate</param>
        /// <param name="y">The y-coordinate</param>
        public Position(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Check whether this position is inside the bounds described by given rectangle.
        /// </summary>
        /// <param name="rectangle">Rectangle providing bounds to check against</param>
        /// <returns>Flag indicating whether positions lies withing given bounds</returns>
        public bool IsInBounds(Rectangle rectangle)
        {
            return (this.X >= rectangle.TopLeft.X && this.X <= rectangle.BottomRight.X)
                   && (this.Y >= rectangle.TopLeft.Y && this.Y <= rectangle.BottomRight.Y);
        }

        #region Equality Implementation
        public bool Equals(Position other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Position other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(Position left, Position right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !left.Equals(right);
        }
        #endregion
        
        #region Arithmetic Operators

        /// <summary>
        /// Add two positions together.
        /// </summary>
        /// <param name="lhs">First position</param>
        /// <param name="rhs">Second position</param>
        /// <returns>Position representing the component-wise sum of the given positions.</returns>
        public static Position operator +(Position lhs, Position rhs)
        {
            return new Position(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }
        
        /// <summary>
        /// Subtract two positions from each other. Note that this might result in negative coordinates.
        /// </summary>
        /// <param name="lhs">First position</param>
        /// <param name="rhs">Second position</param>
        /// <returns>Position representing the component-wise difference of the given positions.</returns>
        public static Position operator -(Position lhs, Position rhs)
        {
            return new Position(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }
        #endregion
        
        #region Implicit Conversion Operators
        
        /// <summary>
        /// Implicit conversion from <see cref="Size"/> to a position instance.
        /// </summary>
        public static implicit operator Position(Size size) => new Position(size.Width, size.Height);

        #endregion
    }
}