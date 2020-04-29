using Engine.Core;

namespace Engine.Graphics
{
    /// <summary>
    /// Represents a set of properties belonging to an ASCII tileset.
    /// </summary>
    public class TilesetProperties
    {
        /// <summary>
        /// Dimensions of a single tile, in pixels, not affected by scaling.
        /// </summary>
        public Size BaseTileDimensions { get; protected set; }

        /// <summary>
        /// Dimensions of a single tile, in pixels, affected by scaling.
        /// </summary>
        public Size TileDimensions => this.BaseTileDimensions * this.ScaleFactor;

        /// <summary>
        /// Scaling factor used to modify tile dimensions
        /// </summary>
        public float ScaleFactor { get; protected set; }
        
        /// <summary>
        /// The dimensions of the ASCII tileset sheet, in glyphs. Note that this is fixed to
        /// 16x16 for now.
        /// </summary>
        public Size TilesetDimensions { get; set; } = new Size(16, 16);

        /// <summary>
        /// Construct a new tileset properties instance from given parameters.
        /// </summary>
        /// <param name="baseTileDimensions">Base dimensions of a single tile, in pixels, unaffected by scaling</param>
        /// <param name="scaleFactor">Scaling factor</param>
        public TilesetProperties(Size baseTileDimensions, float scaleFactor = 1.0f)
        {
            this.BaseTileDimensions = baseTileDimensions;
            this.ScaleFactor = scaleFactor;
        }

        /// <summary>
        /// Calculate the relative pixel coordinates of a given tile position inside a surface that uses a tileset
        /// with properties as stored in this instance.
        /// </summary>
        /// <param name="tilePosition"></param>
        /// <returns></returns>
        public Position TilesToPixels(Position tilePosition)
        {
            return new Position(tilePosition.X * this.TileDimensions.Width, tilePosition.Y * this.TileDimensions.Height);
        }
    }
}