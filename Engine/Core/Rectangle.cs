using System;

namespace Engine.Core
{
    /// <summary>
    /// Represents an immutable rectangle consisting of a top-left position combined with a size. 
    /// </summary>
    public struct Rectangle : IEquatable<Rectangle>
    {
        /// <summary>
        /// The position of the top left corner of the rectangle.
        /// </summary>
        public Position TopLeft { get; }
        
        /// <summary>
        /// Size of the rectangle, containing both width and height.
        /// </summary>
        public Size Size { get; }
        
        /// <summary>
        /// The center of the rectangle.
        /// </summary>
        public Position Center { get; }

        /// <summary>
        /// The position of the bottom right corner of the rectangle.
        /// This is derived from the <see cref="TopLeft"/> and <see cref="Size"/> properties.
        /// </summary>
        public Position BottomRight { get; }
        
        /// <summary>
        /// The position of the bottom right corner of the rectangle.
        /// </summary>
        public Position BottomLeft { get; } 

        /// <summary>
        /// The position of the top right corner of the rectangle.
        /// </summary>
        public Position TopRight { get; }

        /// <summary>
        /// Construct rectangle from given top left corner and size.
        /// </summary>
        /// <param name="topLeft">Position of top left corner</param>
        /// <param name="size">Width and height of the rectangle</param>
        public Rectangle(Position topLeft, Size size)
        {
            this.TopLeft = topLeft;
            this.Size = size;
            this.Center = new Position(this.TopLeft.X + (this.Size.Width / 2), this.TopLeft.Y + (this.Size.Height / 2));
            this.BottomRight = (Size)this.TopLeft + (this.Size - new Size(1, 1));
            this.BottomLeft = new Position(this.TopLeft.X, this.BottomRight.Y);
            this.TopRight = new Position(this.BottomRight.X, this.TopLeft.Y);
        }

        /// <summary>
        /// Create a new rectangle based on the given coordinates of the bounds
        /// </summary>
        public Rectangle(int left, int right, int top, int bottom)
            : this(new Position(left, top), new Position(right, bottom))
        {

        }

        /// <summary>
        /// Construct rectangle from given size. The top left corner is assumed to be at the origin.
        /// </summary>
        /// <param name="size">Width and height of the rectangle</param>
        public Rectangle(Size size)
            : this(Position.Origin, size)
        {
            
        }

        /// <summary>
        /// Construct rectangle from given top left and bottom right corners.
        /// </summary>
        /// <param name="topLeft">Position of top left corner</param>
        /// <param name="bottomRight">Position of bottom right corner</param>
        public Rectangle(Position topLeft, Position bottomRight)
            : this(topLeft, (Size)((bottomRight - topLeft) + new Position(1, 1)))
        {
        }

        /// <summary>
        /// Create rectangle of given size, centered inside this rectangle.
        /// </summary>
        public Rectangle Centered(Size size)
        {
            // Find middle of this rectangle
            var middleOffset = this.Size * 0.5f;
            var middle = this.TopLeft + (Position)middleOffset;

            var halfSize = size * 0.5f;

            var topLeft = middle - (Position)halfSize;
            var bottomRight = middle + (Position)halfSize;
     
            return new Rectangle(topLeft, bottomRight);
        }

        /// <summary>
        /// Create new rectangle which represents this rectangle with given padding applied
        /// </summary>
        public Rectangle WithPadding(Padding padding)
        {
            return new Rectangle(
                this.TopLeft + new Position(padding.Left, padding.Top),
                this.BottomRight - new Position(padding.Right, padding.Bottom)
            );
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