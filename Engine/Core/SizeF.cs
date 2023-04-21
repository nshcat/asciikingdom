using System;
using System.Text.Json.Serialization;

namespace Engine.Core
{
    /// <summary>
    /// Represents a set of floating point dimensions in two dimensions.
    /// </summary>
    public struct SizeF : IEquatable<SizeF>
    {
        /// <summary>
        /// The width value.
        /// </summary>
        public float Width { get; set; }
        
        /// <summary>
        /// The height value.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Empty size, with all dimensions set to zero.
        /// </summary>
        public static SizeF Empty => new SizeF(0, 0);

        /// <summary>
        /// Create a new size instance with given dimension values.
        /// </summary>
        /// <param name="width">Width value</param>
        /// <param name="height">Height value</param>
        public SizeF(float width, float height)
        {
            this.Width = width;
            this.Height = height;
        }

        #region Equality Implementation

        public bool Equals(SizeF other)
        {
            return Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is SizeF other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Width, Height);
        }

        public static bool operator ==(SizeF left, SizeF right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SizeF left, SizeF right)
        {
            return !left.Equals(right);
        }

        #endregion
        
        #region Implicit Conversion Operators
        
        /// <summary>
        /// Implicit conversion from <see cref="Position"/> to a size instance.
        /// </summary>
        public static implicit operator SizeF(Position position) => new SizeF(position.X, position.Y);

        #endregion

        #region Arithmetic Operators

        /// <summary>
        /// Scaling of a size by a decimal factor.
        /// </summary>
        /// <param name="lhs">Dimensions to scale</param>
        /// <param name="rhs">Scaling factor</param>
        /// <returns>New size instance scaled by given factor</returns>
        public static SizeF operator *(SizeF lhs, float rhs)
        {
            var newWidth = (float) lhs.Width * rhs;
            var newHeight = (float) lhs.Height * rhs;

            return new SizeF((int) newWidth, (int) newHeight);
        }

        /// <summary>
        /// Multiply given integral size with given floating point size, component by component
        /// </summary>
        public static SizeF operator *(Size lhs, SizeF rhs)
        {
            var newWidth = (float)lhs.Width * rhs.Width;
            var newHeight = (float)lhs.Height * rhs.Height;

            return new SizeF((int)newWidth, (int)newHeight);
        }

        /// <summary>
        /// Subtract two sizes from each other. This might result in negative components.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static SizeF operator -(SizeF lhs, SizeF rhs)
        {
            return new SizeF(lhs.Width - rhs.Width, lhs.Height - rhs.Height);
        }
        
        /// <summary>
        /// Add two sizes together.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static SizeF operator +(SizeF lhs, SizeF rhs)
        {
            return new SizeF(lhs.Width + rhs.Width, lhs.Height + rhs.Height);
        }

        #endregion
    }
}