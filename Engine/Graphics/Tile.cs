using System;
using System.Text.Json.Serialization;

namespace Engine.Graphics
{
    /// <summary>
    /// Represents all information needed to draw an ASCII tile. Consists of the glyph ID and both front and
    /// back colors.
    /// </summary>
    public struct Tile : IEquatable<Tile>
    {
        /// <summary>
        /// The foreground color.
        /// </summary>
        public Color Front { get; set; }
        
        /// <summary>
        /// The background color.
        /// </summary>
        public Color Back { get; set; }
        
        /// <summary>
        /// The glyph ID.
        /// </summary>
        public int Glyph { get; set; }

        /// <summary>
        /// The empty tile, which just renders as fully black.
        /// </summary>
        [JsonIgnore]
        public Tile Empty => new Tile(0, DefaultColors.Black, DefaultColors.Black);

        /// <summary>
        /// Construct new tile instance with explicitly specified colors.
        /// </summary>
        /// <param name="glyph">The glyph ID</param>
        /// <param name="front">The foreground color</param>
        /// <param name="back">The background color</param>
        public Tile(int glyph, Color front, Color back)
        {
            this.Glyph = glyph;
            this.Front = front;
            this.Back = back;
        }

        /// <summary>
        /// Construct new tile instance with black background.
        /// </summary>
        /// <param name="glyph">The glyph ID</param>
        /// <param name="front">The foreground color</param>
        public Tile(int glyph, Color front)
        {
            this.Glyph = glyph;
            this.Front = front;
            this.Back = DefaultColors.Black;
        }

        /// <summary>
        /// Construct a new tile with given glyph, with white foreground and black background.
        /// </summary>
        /// <param name="glyph">The glyph ID</param>
        public Tile(int glyph)
        {
            this.Glyph = glyph;
            this.Front = DefaultColors.White;
            this.Back = DefaultColors.Black;
        }

        #region Equality Implementation

        public bool Equals(Tile other)
        {
            return Front.Equals(other.Front) && Back.Equals(other.Back) && Glyph == other.Glyph;
        }

        public override bool Equals(object obj)
        {
            return obj is Tile other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Front, Back, Glyph);
        }

        public static bool operator ==(Tile left, Tile right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Tile left, Tile right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}