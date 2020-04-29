using Engine.Core;
using SixLabors.ImageSharp;

namespace Engine.Graphics
{
    /// <summary>
    /// Represents a tileset and its properties
    /// </summary>
    public class Tileset
    {
        /// <summary>
        /// The tileset image.
        /// </summary>
        public Image Image { get; protected set; }
        
        /// <summary>
        /// The shadows texture image.
        /// </summary>
        public Image Shadows { get; protected set; }
        
        /// <summary>
        /// The tilesets properties, such as tile dimensions.
        /// </summary>
        public TilesetProperties Properties { get; protected set; }
        
        // Constructors! Determine properties automatically => just image and scaling factor
        public Tileset(Image image, Image shadows, float scaleFactor)
        {
            this.Image = image;
            this.Shadows = shadows;
            this.Properties = new TilesetProperties(
                new Size(image.Width / 16, image.Height / 16),
                scaleFactor
            );
        }
    }
}