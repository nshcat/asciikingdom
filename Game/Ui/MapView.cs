using System;
using Engine.Core;
using Engine.Graphics;
using Game.Maths;

namespace Game.Ui
{
    /// <summary>
    /// A scene component that allows rendering of a world map.
    /// </summary>
    public class MapView : SceneComponent
    {
        /// <summary>
        /// The map data to draw.
        /// </summary>
        public Tile[,] MapData { get; set; }
        
        /// <summary>
        /// The dimensions of the map to draw, in tiles.
        /// </summary>
        public Size MapDimensions => new Size(this.MapData.GetLength(0), this.MapData.GetLength(1));

        /// <summary>
        /// The map position the cursor is pointing on
        /// </summary>
        public Position CursorPosition { get; set; } = Position.Origin;

        /// <summary>
        /// Whether to draw the cursor
        /// </summary>
        public bool DrawCursor { get; set; } = true;

        /// <summary>
        /// The tile to use to draw the cursor
        /// </summary>
        public Tile CursorTile { get; set; } = new Tile(88, DefaultColors.Yellow, DefaultColors.Black);
        
        /// <summary>
        /// Construct new map view instance.
        /// </summary>
        public MapView(Position position, Size dimensions, Tile[,] mapData) : base(position, dimensions)
        {
            this.MapData = mapData;
            this.Recenter();
        }

        /// <summary>
        /// Centers the view to the center of the map
        /// </summary>
        public void Recenter()
        {
            this.CursorPosition = new Rectangle(this.MapDimensions).Center;
        }

        /// <summary>
        /// Move center up by <see cref="steps"/> tiles
        /// </summary>
        public void Up(int steps = 1)
        {
            this.CursorPosition = new Position(
                this.CursorPosition.X,
                MathUtil.Clamp(this.CursorPosition.Y - steps, 0, this.MapDimensions.Height - 1)
            );
        }
        
        /// <summary>
        /// Move center down by <see cref="steps"/> tiles
        /// </summary>
        public void Down(int steps = 1)
        {
            this.CursorPosition = new Position(
                this.CursorPosition.X,
                MathUtil.Clamp(this.CursorPosition.Y + steps, 0, this.MapDimensions.Height - 1)
            );
        }
        
        /// <summary>
        /// Move center left by <see cref="steps"/> tiles
        /// </summary>
        public void Left(int steps = 1)
        {
            this.CursorPosition = new Position(
                MathUtil.Clamp(this.CursorPosition.X - steps, 0, this.MapDimensions.Width - 1),
                this.CursorPosition.Y
            );
        }
        
        /// <summary>
        /// Move center left by <see cref="steps"/> tiles
        /// </summary>
        public void Right(int steps = 1)
        {
            this.CursorPosition = new Position(
                MathUtil.Clamp(this.CursorPosition.X + steps, 0, this.MapDimensions.Width - 1),
                this.CursorPosition.Y
            );
        }

        /// <summary>
        /// Draw map view
        /// </summary>
        public override void Render(Surface surface)
        {
            var halfDimensions = new Position(this.Dimensions.Width / 2, this.Dimensions.Height / 2);
            
            // The view prefers the cursor position to be in the center of the screen, so calculate
            // what map position the top left corner of the view is associated with
            var topLeft =
                this.CursorPosition - halfDimensions;
            
            // Clamp it so we do not run out of the map
            var finalTopLeft = new Position(
                Math.Max(0, Math.Min(this.MapDimensions.Width - 1 - this.Dimensions.Width, topLeft.X)),
                Math.Max(0, Math.Min(this.MapDimensions.Height - 1 - this.Dimensions.Height, topLeft.Y))
            );
            
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var localPosition = new Position(ix, iy);
                    var mapPosition = localPosition + finalTopLeft;
                    var screenPosition = localPosition + this.Position;

                    if ((mapPosition.X < this.MapDimensions.Width && mapPosition.Y < this.MapDimensions.Height)
                        && (mapPosition.X >= 0 && mapPosition.Y >= 0) 
                        && screenPosition.IsInBounds(surface.Dimensions))
                    {
                        surface.SetTile(screenPosition, this.MapData[mapPosition.X, mapPosition.Y]);
                    }
                }
            }

            if (this.DrawCursor)
            {
                var x = this.Position.X + halfDimensions.X;

                if (topLeft.X < 0)
                {
                    x += topLeft.X;
                }
                else if (topLeft.X >= this.MapDimensions.Width - 1 - this.Dimensions.Width)
                {
                    x += (topLeft.X - (this.MapDimensions.Width - 1 - this.Dimensions.Width)) - 1;
                }

                var y = this.Position.Y + halfDimensions.Y;
                
                if (topLeft.Y < 0)
                {
                    y += topLeft.Y;
                }
                else if (topLeft.Y >= this.MapDimensions.Height - 1 - this.Dimensions.Height)
                {
                    y += (topLeft.Y - (this.MapDimensions.Height - 1 - this.Dimensions.Height)) - 1;
                }

                surface.SetTile(new Position(x, y), this.CursorTile);
            }
        }
    }
}