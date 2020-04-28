using System;
using System.Globalization;

namespace Engine.Graphics
{
    /// <summary>
    /// Represents a 24-bit integral RGB color triple.
    /// </summary>
    public struct Color : IEquatable<Color>
    {
        /// <summary>
        /// The red color channel.
        /// </summary>
        public int R { get; set; }
        
        /// <summary>
        /// The green color channel.
        /// </summary>
        public int G { get; set; }
        
        /// <summary>
        /// The blue color channel.
        /// </summary>
        public int B { get; set; }

        /// <summary>
        /// Construct new color instance from given integral color channel values in range [0, 255].
        /// </summary>
        /// <param name="r">Red color channel value</param>
        /// <param name="g">Green color channel value</param>
        /// <param name="b">Blue color channel value</param>
        public Color(int r, int g, int b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }

        /// <summary>
        /// Create color instance from given color hex code string.
        /// </summary>
        /// <param name="hexcode">Color hex code, with leading '#'</param>
        public static Color FromHex(string hexcode)
        {
            // Parse hex code as packed RGB value
            var rgb = int.Parse(hexcode.Replace("#", ""), NumberStyles.HexNumber);

            return FromRgb(rgb);
        }

        /// <summary>
        /// Create color instance from given packed RGB value. 
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static Color FromRgb(int rgb)
        {
            var r = (rgb & 0xFF000) >> 16;
            var g = (rgb & 0xFF00) >> 8;
            var b = (rgb & 0xFF);

            return new Color(r, g, b);
        }

        #region Equality Implementation

        public bool Equals(Color other)
        {
            return R == other.R && G == other.G && B == other.B;
        }

        public override bool Equals(object obj)
        {
            return obj is Color other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B);
        }

        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Color left, Color right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}