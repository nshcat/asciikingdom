using System;
using Engine.Core;
using Engine.Resources;

namespace Engine.Graphics
{
    /// <summary>
    /// Class implementing the builder pattern to allow more comfortable surface creation
    /// </summary>
    public class SurfaceBuilder
    {
        private Tileset _tileset = null;
        private Position _topleft = Position.Origin;
        private Size _dimensions = Size.Empty;
        private bool _transparent = false;
        
        /// <summary>
        /// Set the tileset the surface should use
        /// </summary>
        /// <param name="tileset">Tileset to use</param>
        public SurfaceBuilder Tileset(Tileset tileset)
        {
            this._tileset = tileset;
            return this;
        }

        /// <summary>
        /// Retrieve tileset from resources and use it for this surface
        /// </summary>
        /// <param name="resourceManager">Resource manager to retrieve tileset from</param>
        /// <param name="tilesetName">Name of the tileset to retrieve</param>
        /// <param name="scaleFactor">Scaling factor to use</param>
        /// <returns></returns>
        public SurfaceBuilder Tileset(ResourceManager resourceManager, string tilesetName, float scaleFactor = 1.0f)
        {
            var tileset = resourceManager.GetTileset(tilesetName, scaleFactor);
            this._tileset = tileset;
            return this;
        }

        /// <summary>
        /// Set surface position relative to parent surface
        /// </summary>
        /// <param name="parent">Parent surface</param>
        /// <param name="relativeTopLeft">Relative position on parent surface, in tiles</param>
        public SurfaceBuilder RelativeTo(Surface parent, Position relativeTopLeft)
        {
            // Convert relative tile position to relative pixel position
            var relativePixel = parent.Tileset.Properties.TilesToPixels(relativeTopLeft);
            
            // Calculate absolute screen position of new surface, in pixels
            this._topleft = parent.TopLeft + relativePixel;
            
            return this;
        }

        /// <summary>
        /// Set surface position relative to parent surface, and derive surface dimensions from bottom right
        /// point relative to parent surface.
        ///
        /// This method requires that the tileset has already been set.
        /// 
        /// This will calculate pixel coordinates that are _inclusive_ of both points:
        ///
        ///  TL OXXXX
        ///     XXXXX
        ///     XXXXO BR
        /// </summary>
        /// <param name="parent">Parent surface</param>
        /// <param name="relativeTopLeft">Top left position on parent surface, in tiles</param>
        /// <param name="relativeBottomRight">Bottom right position on parent surface, in tiles</param>
        public SurfaceBuilder RelativeTo(Surface parent, Position relativeTopLeft, Position relativeBottomRight)
        {
            if(this._tileset == null)
                throw new InvalidOperationException("Tileset isn't set");

            var topLeftPixels = parent.Tileset.Properties.TilesToPixels(relativeTopLeft);
            var bottomRightPixels = parent.Tileset.Properties.TilesToPixels(relativeBottomRight);
            
            // Determine width and height of the surface, in pixels
            // To fully include the BR point, we need to add the dimensions of the parent surfaces glyphs here.
            var dimensionsPixels = new Size(
                (bottomRightPixels.X - topLeftPixels.X) + parent.Tileset.Properties.TileDimensions.Width,
                (bottomRightPixels.Y - topLeftPixels.Y) + parent.Tileset.Properties.TileDimensions.Height
            );
            
            // Find the nearest number of tiles that fit into it
            this._dimensions = new Size(
                dimensionsPixels.Width / this._tileset.Properties.TileDimensions.Width,
                dimensionsPixels.Height / this._tileset.Properties.TileDimensions.Height
            );

            this._topleft = parent.TopLeft + topLeftPixels;
            
            return this;
        }

        /// <summary>
        /// Set surface position relative to parent surface
        /// </summary>
        /// <param name="parent">Parent surface</param>
        /// <param name="relativeX">X-coordinate of relative position on parent surface, in tiles</param>
        /// <param name="relativeY">Y-coordinate of relative position on parent surface, in tiles</param>
        public SurfaceBuilder RelativeTo(Surface parent, int relativeX, int relativeY)
        {
            return this.RelativeTo(parent, new Position(relativeX, relativeY));
        }
        
        /// <summary>
        /// Set the dimensions of the surface, in tiles
        /// </summary>
        /// <param name="dimensions">Dimensions of the surface, in tiles</param>
        public SurfaceBuilder TileDimensions(Size dimensions)
        {
            this._dimensions = dimensions;
            return this;
        }
        
        /// <summary>
        /// Set the dimensions of the surface, in tiles
        /// </summary>
        /// <param name="width">Width of the surface, in tiles</param>
        /// <param name="height">Height of the surface, in tiles</param>
        public SurfaceBuilder TileDimensions(int width, int height)
        {
            return this.TileDimensions(new Size(width, height));
        }

        /// <summary>
        /// Set top left position of surface on the screen, in pixels.
        /// </summary>
        /// <param name="topLeft">Top left position of surface, in pixels</param>
        public SurfaceBuilder TopLeft(Position topLeft)
        {
            this._topleft = topLeft;
            return this;
        }

        /// <summary>
        /// Set top left position of surface on the screen, in pixels.
        /// </summary>
        /// <param name="x">X-coordinate of top left position of surface, in pixels</param>
        /// <param name="y">Y-coordinate of top left position of surface, in pixels</param>
        public SurfaceBuilder TopLeft(int x, int y)
        {
            return this.TopLeft(new Position(x, y));
        }

        /// <summary>
        /// Set the dimensions of the surface, in pixels. Note that this requires that the tileset has already
        /// been set.
        /// </summary>
        /// <param name="dimensions">Dimensions of the surface, in pixels</param>
        public SurfaceBuilder PixelDimensions(Size dimensions)
        {
            if(this._tileset == null)
                throw new InvalidOperationException("Can't specify pixel dimensions when tileset isn't set");

            var tileDimensions = this._tileset.Properties.TileDimensions;
            
            this._dimensions = new Size(
                dimensions.Width / tileDimensions.Width,
                dimensions.Height / tileDimensions.Height
            );
            
            return this;
        }

        /// <summary>
        /// Set the dimensions of the surface, in pixels. Note that this requires that the tileset has already
        /// been set.
        /// </summary>
        /// <param name="width">Width of the surface, in pixels</param>
        /// <param name="height">Height of the surface, in pixels</param>
        public SurfaceBuilder PixelDimensions(int width, int height)
        {
            return this.PixelDimensions(new Size(width, height));
        }
        
        /// <summary>
        /// Finish surface creation and return instance
        /// </summary>
        /// <returns>New surface instance</returns>
        public Surface Build()
        {
            if(this._tileset == null)
                throw new InvalidOperationException("Tileset was not supplied");
            
            if(this._dimensions.IsEmpty)
                throw new InvalidOperationException("Surface dimensions must not be empty");
            
            return new Surface(this._topleft, this._dimensions, this._tileset)
            {
                IsTransparent = this._transparent
            };
        }
        
    }
}