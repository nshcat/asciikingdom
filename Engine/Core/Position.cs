using System;
using System.Text.Json.Serialization;

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
        [JsonIgnore]
        public bool IsOrigin => this.X == 0 && this.Y == 0;

        /// <summary>
        /// The origin position, with both coordinates set to zero.
        /// </summary>
        public static Position Origin => new Position(0, 0);
        
        /// <summary>
        /// Delta position north
        /// </summary>
        public static Position North => new Position(0, -1);
        
        /// <summary>
        /// Delta position south
        /// </summary>
        public static Position South => new Position(0, 1);
        
        /// <summary>
        /// Delta position west
        /// </summary>
        public static Position West => new Position(-1, 0);
        
        /// <summary>
        /// Delta position east
        /// </summary>
        public static Position East => new Position(1, 0);

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
        
        /// <summary>
        /// Check whether this position is inside the bounds described by given size.
        /// </summary>
        /// <param name="size">Size providing bounds to check against</param>
        /// <returns>Flag indicating whether positions lies withing given bounds</returns>
        public bool IsInBounds(Size size)
        {
            return (this.X >= 0 && this.X < size.Width)
                   && (this.Y >= 0 && this.Y < size.Height);
        }

        /// <summary>
        /// Calculate the distance between two points.
        /// </summary>
        public static float GetDistance(Position lhs, Position rhs)
        {
            return (float) Math.Sqrt(
                Math.Pow(rhs.X - lhs.X, 2.0)
                + Math.Pow(rhs.Y - lhs.Y, 2.0)
            );
        }

        /// <summary>
        /// Create new position with clamped coordinates
        /// </summary>
        /// <param name="min">Minimum values for the two coordinates</param>
        /// <param name="max">Maximum values for the two coordinates</param>
        /// <returns></returns>
        public Position Clamp(Position min, Position max)
        {
            return new Position(
                Math.Max(min.X, Math.Min(max.X, this.X)),
                Math.Max(min.Y, Math.Min(max.Y, this.Y))
            );
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
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
        
        /// <summary>
        /// Multiply position components with given factor.
        /// </summary>
        public static Position operator *(Position lhs, int f)
        {
            return new Position(lhs.X * f, lhs.Y * f);
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