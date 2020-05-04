using System;
using Engine.Core;
using Engine.Graphics;

namespace Game.Ui
{
    /// <summary>
    /// A scene component that allow rendering of a world map.
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
        /// The map position the center of the screen is focused on
        /// </summary>
        public Position Center { get; set; } = Position.Origin;
        
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
            this.Center = new Rectangle(this.MapDimensions).Center;
        }

        /// <summary>
        /// Move center up by one tile.
        /// </summary>
        public void Up()
        {
            if (this.Center.Y != 0)
            {
                this.Center += new Position(0, -1);
            }
        }
        
        /// <summary>
        /// Move center down by one tile.
        /// </summary>
        public void Down()
        {
            if (this.Center.Y != this.MapDimensions.Height - 1)
            {
                this.Center += new Position(0, 1);
            }
        }
        
        /// <summary>
        /// Move center left by one tile.
        /// </summary>
        public void Left()
        {
            if (this.Center.X != 0)
            {
                this.Center += new Position(-1, 0);
            }
        }
        
        /// <summary>
        /// Move center left by one tile.
        /// </summary>
        public void Right()
        {
            if (this.Center.X != this.MapDimensions.Width - 1)
            {
                this.Center += new Position(1, 0);
            }
        }

        /// <summary>
        /// Draw map view
        /// </summary>
        public override void Render(Surface surface)
        {
            var shift = new Position(
                this.Center.X - (int)(this.Dimensions.Width / 2),
                this.Center.Y - (int)(this.Dimensions.Height / 2)
            );

            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var localPosition = new Position(ix, iy);
                    var mapPosition = localPosition + shift;
                    var screenPosition = localPosition + this.Position;

                    if ((mapPosition.X < this.MapDimensions.Width && mapPosition.Y < this.MapDimensions.Height)
                        && (mapPosition.X >= 0 && mapPosition.Y >= 0) 
                        && screenPosition.IsInBounds(surface.Dimensions))
                    {
                        surface.SetTile(screenPosition, this.MapData[mapPosition.X, mapPosition.Y]);
                    }
                }
            }
        }
    }
}