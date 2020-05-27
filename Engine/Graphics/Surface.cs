using System;
using Engine.Core;
using Engine.Rendering;
using Engine.Resources;
using OpenToolkit.Graphics.OpenGL;
using GL = OpenToolkit.Graphics.OpenGL4.GL;
using PrimitiveType = OpenToolkit.Graphics.OpenGL4.PrimitiveType;
using TextureUnit = OpenToolkit.Graphics.OpenGL4.TextureUnit;

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
        /// Relative bounds of this surface for bounds checks of relative tile positions of the surface.
        /// </summary>
        public Rectangle Bounds => new Rectangle(Position.Origin, this.Dimensions);
        
        /// <summary>
        /// Absolute bounds of this surface for bounds checks of global screen coordinates
        /// </summary>
        public Rectangle AbsoluteBounds => new Rectangle(this.TopLeft, this.Dimensions);

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
        public bool IsEnabled { get; set; } = true;
        
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
        /// Empty VAO and VBO used to work around bugs on some GPUs that cause
        /// the no-geometry rendering of surfaces to not work
        /// </summary>
        protected EmptyBuffers EmptyBuffers { get; set; } = new EmptyBuffers();
        
        /// <summary>
        /// The actual screen data.
        /// </summary>
        protected int[] Data { get; set; }
        
        /// <summary>
        /// The material used to render this surface
        /// </summary>
        protected AsciiMaterial Material { get; set; } = new AsciiMaterial();
        
        /// <summary>
        /// The tileset texture
        /// </summary>
        protected Texture2D TilesetTexture { get; set; }
        
        /// <summary>
        /// The shadow texture
        /// </summary>
        protected Texture2D ShadowTexture { get; set; }

        #endregion
        
        #region Offsets

        /// <summary>
        /// Enumeration containing the offsets into the screen buffer data packets
        /// </summary>
        protected enum Offset
        {
            FrontRed = 0,
            FrontGreen = 1,
            FrontBlue = 2,
            Glyph = 3,
            BackRed = 4,
            BackGreen = 5,
            BackBlue = 6,
            Data = 7
        }
        
        #endregion

        /// <summary>
        /// Create a new surface.
        /// </summary>
        /// <param name="topLeft">Absolute screen position of the top left corner, in pixels</param>
        /// <param name="dimensions">Dimensions of the surface, in glyphs</param>
        /// <param name="tileset">Tileset to use</param>
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
            
            this.TilesetTexture = new Texture2D(this.Tileset.Image);
            this.ShadowTexture = new Texture2D(this.Tileset.Shadows);
        }

        /// <summary>
        /// Create new surface using a surface builder.
        /// </summary>
        /// <returns>New surface builder instance</returns>
        public static SurfaceBuilder New()
        {
            return new SurfaceBuilder();
        }

        #region Modification Methods

        /// <summary>
        /// Set transparency flag for surface tile at given position.
        /// </summary>
        /// <param name="position">Tile position on surface</param>
        /// <param name="isTransparent">Transparency flag</param>
        public void SetTransparent(Position position, bool isTransparent)
        {
            if (!position.IsInBounds(this.Bounds))
                return;
            
            this.IsDirty = true;
            var bit = isTransparent ? 0x1 : 0x0;
            var offset = this.OffsetOf(position, Offset.Data);

            this.Data[offset] = (this.Data[offset] & 0xFF7F) | (bit << 7);
        }

        /// <summary>
        /// Set depth value for surface tile at given position
        /// </summary>
        /// <param name="position">Tile position on surface</param>
        /// <param name="depth">Depth value</param>
        public void SetDepth(Position position, int depth)
        {
            if (!position.IsInBounds(this.Bounds))
                return;

            if (depth < 0 || depth > 31)
                throw new ArgumentException("Depth value needs to bin range [0, 32)");

            var maskedDepth = depth & 0x3F;
            var offset = this.OffsetOf(position, Offset.Data);
            this.Data[offset] = (this.Data[offset] & 0xFFC0) | maskedDepth;
            
            this.IsDirty = true;
        }

        /// <summary>
        /// Set whether given tile has an UI shadow on it
        /// </summary>
        /// <param name="position">Tile position on surface</param>
        public void SetUiShadow(Position position, bool flag)
        {
            if (!position.IsInBounds(this.Bounds))
                return;
            
            this.IsDirty = true;
            var bit = flag ? 0x1 : 0x0;
            var offset = this.OffsetOf(position, Offset.Data);

            this.Data[offset] = (this.Data[offset] & 0xFFBF) | (bit << 6);
        }

        /// <summary>
        /// Set foreground color at given position
        /// </summary>
        /// <param name="position">Tile position on surface</param>
        /// <param name="color">New foreground</param>
        public void SetForeground(Position position, Color color)
        {
            this.Data[this.OffsetOf(position, Offset.FrontRed)] = color.R;
            this.Data[this.OffsetOf(position, Offset.FrontGreen)] = color.G;
            this.Data[this.OffsetOf(position, Offset.FrontBlue)] = color.B;
            this.IsDirty = true;
            this.SetTransparent(position, false);
        }
        
        /// <summary>
        /// Set background color at given position
        /// </summary>
        /// <param name="position">Tile position on surface</param>
        /// <param name="color">New background color</param>
        public void SetBackground(Position position, Color color)
        {
            this.Data[this.OffsetOf(position, Offset.BackRed)] = color.R;
            this.Data[this.OffsetOf(position, Offset.BackGreen)] = color.G;
            this.Data[this.OffsetOf(position, Offset.BackBlue)] = color.B;
            this.IsDirty = true;
            this.SetTransparent(position, false);
        }

        /// <summary>
        /// Set glyph at given position
        /// </summary>
        /// <param name="position">Tile position on surface</param>
        /// <param name="glyph">New glyph ID</param>
        public void SetGlyph(Position position, int glyph)
        {
            if (glyph < 0 || glyph > 255)
                throw new ArgumentException("Glyph ID out of range");

            this.Data[this.OffsetOf(position, Offset.Glyph)] = glyph;
            this.IsDirty = true;
            this.SetTransparent(position, false);
        }
        
        /// <summary>
        /// Set tile at given position. Will do nothing if the position is out of bounds.
        /// </summary>
        /// <param name="position">Tile position on surface</param>
        /// <param name="tile">New tile data</param>
        public void SetTile(Position position, Tile tile)
        {
            if (!position.IsInBounds(this.Bounds))
                return;

            this.SetGlyph(position, tile.Glyph);
            this.SetForeground(position, tile.Front);
            this.SetBackground(position, tile.Back);
        }

        /// <summary>
        /// Clears the surface. Depending on the <see cref="IsTransparent"/> property, this will either
        /// fill it with black or transparency.
        /// </summary>
        public void Clear()
        {
            Array.Fill(this.Data, 0);
            this.IsDirty = true;

            if (this.IsTransparent)
            {
                for (int ix = 0; ix < this.Dimensions.Width; ++ix)
                {
                    for (int iy = 0; iy < this.Dimensions.Height; ++iy)
                    {
                        this.SetTransparent(new Position(ix, iy), true);
                    }
                }
            }
        }

        /// <summary>
        /// Destroy the GPU objects associated with this surface.
        /// </summary>
        public void Destroy()
        {
            this.TilesetTexture.Destroy();
            this.ShadowTexture.Destroy();
            this.BufferTexture.Destroy();
        }

        #endregion

        /// <summary>
        /// Render the surface to screen.
        /// </summary>
        /// <param name="rp">Rendering parameters to use</param>
        public void Render(RenderParams rp)
        {
            // Only render if surface is enabled
            if (!this.IsEnabled)
                return;
            
            // Activate material
            this.Material.Use();
            this.Material.ApplyParameters(rp);
            
            // Upload GPU data if needed
            if (this.IsDirty)
            {
                this.IsDirty = false;
                this.BufferTexture.Upload(this.Data);
            }
            
            // Activate empty buffers
            this.EmptyBuffers.Use();
            
            // Activate all textures
            this.TilesetTexture.Use(TextureUnit.Texture0);
            this.ShadowTexture.Use(TextureUnit.Texture1);
            this.BufferTexture.Use(TextureUnit.Texture2);
            
            // Render instanced quad for each tile on the surface
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, this.Dimensions.Width * this.Dimensions.Height);
        }

        /// <summary>
        /// Determine the linear offset into the data buffer for given tile position
        /// </summary>
        /// <param name="position">Tile position on surface</param>
        /// <returns>Linear tile offset into data buffer</returns>
        protected int OffsetOf(Position position)
        {
            return (2 * 4) * ((this.Dimensions.Width * position.Y) + position.X);
        }
        
        /// <summary>
        /// Determine the linear offset into the data buffer for given tile position and internal
        /// offset
        /// </summary>
        /// <param name="position">Tile position on surface</param>
        /// <param name="type">Offset type inside the tile data packet</param>
        /// <returns>Linear tile offset into data buffer</returns>
        protected int OffsetOf(Position position, Offset type)
        {
            return this.OffsetOf(position) + (int) type;
        }
    }
}