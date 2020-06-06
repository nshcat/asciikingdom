using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenToolkit.Mathematics;

namespace Engine.Graphics
{
    /// <summary>
    /// JSON converter for colors
    /// </summary>
    public class ColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Color.FromHex(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToHex());
        }
    }
    
    /// <summary>
    /// Represents a 24-bit integral RGB color triple.
    /// </summary>
    [JsonConverter(typeof(ColorConverter))]
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
        /// Convert integral color instance to floating point RGBA color tuple.
        /// The alpha value will always be set to 1.0f.
        /// </summary>
        /// <returns>Floating point RGBA color tuple corresponding to instance</returns>
        public Vector4 ToVector4F()
        {
            return new Vector4(
                (float)this.R / 255.0f,
                (float)this.G / 255.0f,
                (float)this.B / 255.0f,
                1.0f
            );
        }

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
        /// Create color instance with given grayscale value
        /// </summary>
        /// <param name="value">Grayscale value to use, in [0, 1]</param>
        public static Color FromGrayscale(float value)
        {
            var scaled = (int) (value * 255.0f);

            return new Color(scaled, scaled, scaled);
        }

        /// <summary>
        /// Create color instance from given packed RGB value. 
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static Color FromRgb(int rgb)
        {
            var r = (rgb & 0xFF0000) >> 16;
            var g = (rgb & 0xFF00) >> 8;
            var b = (rgb & 0xFF);

            return new Color(r, g, b);
        }

        /// <summary>
        /// Perform linear interpolation between two colors.
        /// </summary>
        /// <param name="value">Interpolation factor, in [0, 1]</param>
        /// <param name="a">First color</param>
        /// <param name="b">Second color</param>
        public static Color Lerp(float value, Color a, Color b)
        {
            Func<float, int, int, int> lerpChannel = (float val, int a, int b) =>
            {
                var result = (1.0f - val) * (float) a + val * (float) b;
                return (int) result;
            };

            return new Color(
                lerpChannel(value, a.R, b.R),
                lerpChannel(value, a.G, b.G),
                lerpChannel(value, a.B, b.B)
            );
        }

        /// <summary>
        /// Convert color to hex string
        /// </summary>
        /// <returns>Hex string representation of this color, including preceding '#'</returns>
        public string ToHex()
        {
            return $"#{this.R:X2}#{this.G:X2}#{this.B:X2}";
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