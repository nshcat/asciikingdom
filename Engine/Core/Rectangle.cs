using System;

namespace Engine.Core
{
    /// <summary>
    /// Represents a rectangle consisting of a top-left position combined with a size.
    /// </summary>
    public struct Rectangle : IEquatable<Rectangle>
    {
        /// <summary>
        /// The position of the top left corner of the rectangle.
        /// </summary>
        public Position TopLeft { get; set; }
        
        /// <summary>
        /// Size of the rectangle, containing both width and height.
        /// </summary>
        public Size Size { get; set; }
        
        /// <summary>
        /// The center of the rectangle.
        /// </summary>
        public Position Center => new Position(this.TopLeft.X + (this.Size.Width / 2), this.TopLeft.Y + (this.Size.Height / 2));

        /// <summary>
        /// The position of the bottom right corner of the rectangle.
        /// This is derived from the <see cref="TopLeft"/> and <see cref="Size"/> properties.
        /// </summary>
        public Position BottomRight => (Size)this.TopLeft + (this.Size - new Size(1, 1));

        /// <summary>
        /// Construct rectangle from given top left corner and size.
        /// </summary>
        /// <param name="topLeft">Position of top left corner</param>
        /// <param name="size">Width and height of the rectangle</param>
        public Rectangle(Position topLeft, Size size)
        {
            this.TopLeft = topLeft;
            this.Size = size;
        }

        /// <summary>
        /// Construct rectangle from given size. The top left corner is assumed to be at the origin.
        /// </summary>
        /// <param name="size">Width and height of the rectangle</param>
        public Rectangle(Size size) : this(Position.Origin, size)
        {
            
        }

        /// <summary>
        /// Construct rectangle from given top left and bottom right corners.
        /// </summary>
        /// <param name="topLeft">Position of top left corner</param>
        /// <param name="bottomRight">Position of bottom right corner</param>
        public Rectangle(Position topLeft, Position bottomRight)
        {
            this.TopLeft = topLeft;
            this.Size = (bottomRight - topLeft) + new Position(1, 1);
        }
        
        #region Equality Implementation

        public bool Equals(Rectangle other)
        {
            return TopLeft.Equals(other.TopLeft) && Size.Equals(other.Size);
        }

        public override bool Equals(object obj)
        {
            return obj is Rectangle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TopLeft, Size);
        }

        public static bool operator ==(Rectangle left, Rectangle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rectangle left, Rectangle right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}