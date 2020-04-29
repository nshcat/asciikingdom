using Engine.Core;
using Engine.Rendering;
using Engine.Resources;
using OpenToolkit.Graphics.OpenGL4;

namespace Engine.Graphics
{
    /// <summary>
    /// A surface that can be drawn on by the game. Since each surface can use different tile sets,
    /// the game can use different glyph styles and, more importantly, sizes for different parts of the
    /// user interface.
    ///
    /// Once a surface is created, its absolute position and size can not be changed anymore.
    /// </summary>
    public class Surface : IRenderable
    {
        #region Public Properties
        
        /// <summary>
        /// The tileset used by this surface
        /// </summary>
        public Tileset Tileset { get; protected set; }

        /// <summary>
        /// The absolute screen position of the top left corner, in pixels
        /// </summary>
        public Position TopLeft { get; protected set; }

        /// <summary>
        /// The dimensions of the surface, in tiles
        /// </summary>
        public Size Dimensions { get; protected set; }

        /// <summary>
        /// The dimensions of the surface, in pixels. Derived from the <see cref="Dimensions"/> property.
        /// </summary>
        public Size PixelDimensions => new Size(
            this.Dimensions.Width * this.Tileset.Properties.TileDimensions.Width,
            this.Dimensions.Height * this.Tileset.Properties.TileDimensions.Height
        );
        
        /// <summary>
        /// Whether this surfaces background is transparent by default.
        /// </summary>
        public bool IsTransparent { get; set; }
        
        /// <summary>
        /// Whether this surface is enabled. This controls whether it is drawn or not.
        /// </summary>
        public bool IsEnabled { get; set; }
        
        #endregion

        #region Rendering Properties

        /// <summary>
        /// Whether the state of this surface is considered to be dirty, and thus has to be synced
        /// with the GPU before rendering.
        /// </summary>
        protected bool IsDirty { get; set; } = true;
        
        /// <summary>
        /// The size of the internal integer buffer holding the screen state.
        /// </summary>
        protected int BufferSize { get; set; }
        
        /// <summary>
        /// The buffer texture that stores the screen data on the GPU
        /// </summary>
        protected BufferTexture BufferTexture { get; set; }
        
        /// <summary>
        /// The actual screen data.
        /// </summary>
        protected int[] Data { get; set; }
        
        /// <summary>
        /// The material used to render this surface
        /// </summary>
        protected AsciiMaterial Material { get; set; } = new AsciiMaterial();

        #endregion

        /// <summary>
        /// Create a new surface.
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="dimensions"></param>
        /// <param name="tileset"></param>
        public Surface(
            Position topLeft,
            Size dimensions,
            Tileset tileset)
        {
            this.Tileset = tileset;
            this.Dimensions = dimensions;
            this.TopLeft = topLeft;

            this.Material.SurfaceDimensions = this.Dimensions;
            this.Material.TopLeft = this.TopLeft;
            this.Material.TilesetInfo = this.Tileset.Properties;

            this.BufferSize = this.Dimensions.Height * this.Dimensions.Width * 2 * 4;
            this.BufferTexture = new BufferTexture(this.BufferSize);
            this.Data = new int[this.BufferSize];
        }

        #region Modification Methods

        /// <summary>
        /// Clears the surface. Depending on the <see cref="IsTransparent"/> property, this will either
        /// fill it with black or transparency.
        /// </summary>
        public void Clear()
        {
            
        }

        /// <summary>
        /// Destroy the GPU objects associated with this surface.
        /// </summary>
        public void Destroy()
        {
            this.BufferTexture.Destroy();
        }

        #endregion

        /// <summary>
        /// Render the surface to screen.
        /// </summary>
        /// <param name="rp">Rendering parameters to use</param>
        public void Render(RenderParams rp)
        {
            // Activate material
            this.Material.Use();
            this.Material.ApplyParameters(rp);
            
            // Upload GPU data if needed
            if (this.IsDirty)
            {
                this.IsDirty = false;
                this.BufferTexture.Upload(this.Data);
            }
            
            // Activate all textures
            // TODO other textures
            this.BufferTexture.Use(TextureUnit.Texture2);
            
            // Render instanced quad for each tile on the surface
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, this.Dimensions.Width * this.Dimensions.Height);
        }
    }
}