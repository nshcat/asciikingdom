using System;

namespace Engine.Core
{
    /// <summary>
    /// Represents a set of integral dimensions in two dimensions.
    /// </summary>
    public struct Size : IEquatable<Size>
    {
        /// <summary>
        /// The width value.
        /// </summary>
        public int Width { get; set; }
        
        /// <summary>
        /// The height value.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Whether this size represents the empty size, with all dimensions set to zero.
        /// </summary>
        public bool IsEmpty => this.Width == 0 && this.Height == 0;
        
        /// <summary>
        /// Empty size, with all dimensions set to zero.
        /// </summary>
        public Size Empty => new Size(0, 0);

        /// <summary>
        /// Create a new size instance with given dimension values.
        /// </summary>
        /// <param name="width">Width value</param>
        /// <param name="height">Height value</param>
        public Size(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        #region Equality Implementation

        public bool Equals(Size other)
        {
            return Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is Size other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Width, Height);
        }

        public static bool operator ==(Size left, Size right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Size left, Size right)
        {
            return !left.Equals(right);
        }

        #endregion
        
        #region Implicit Conversion Operators
        
        /// <summary>
        /// Implicit conversion from <see cref="Position"/> to a size instance.
        /// </summary>
        public static implicit operator Size(Position position) => new Size(position.X, position.Y);

        #endregion

        #region Arithmetic Operators

        /// <summary>
        /// Scaling of a size by a decimal factor.
        /// </summary>
        /// <param name="lhs">Dimensions to scale</param>
        /// <param name="rhs">Scaling factor</param>
        /// <returns>New size instance scaled by given factor</returns>
        public static Size operator *(Size lhs, float rhs)
        {
            var newWidth = (float) lhs.Width * rhs;
            var newHeight = (float) lhs.Height * rhs;

            return new Size((int) newWidth, (int) newHeight);
        }

        #endregion
    }
}