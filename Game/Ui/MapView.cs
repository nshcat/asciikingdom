using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Engine.Core;
using Engine.Graphics;
using Game.Core;
using Game.Maths;
using Game.Simulation;

namespace Game.Ui
{
    /// <summary>
    /// Represents the different map components a map view can display
    /// </summary>
    public enum MapViewMode
    {
        Terrain,
        Temperature,
        Rainfall,
        Drainage
    }
    
    /// <summary>
    /// A scene component that allows rendering of a world map.
    /// </summary>
    public class MapView : SceneComponent, ILogic
    {
        /// <summary>
        /// The map to draw
        /// </summary>
        public Map Map { get; set; }

        /// <summary>
        /// What map component to display
        /// </summary>
        public MapViewMode DisplayMode { get; set; } = MapViewMode.Terrain;

        /// <summary>
        /// Whether this view currently is associated with any map data.
        /// </summary>
        public bool HasMapData => this.Map != null;

        /// <summary>
        /// The dimensions of the map to draw, in tiles.
        /// </summary>
        public Size MapDimensions => this.Map.Dimensions;

        /// <summary>
        /// Whether to show special resources on the map
        /// </summary>
        public bool ShowResources { get; set; } = true;

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
        /// Timer used to blink cursor
        /// </summary>
        protected ToggleTimer CursorTimer { get; set; } = new ToggleTimer(0.25, true);

        /// <summary>
        /// The map data to visualize. This depends on the current <see cref="DisplayMode"/>.
        /// </summary>
        protected Tile[,] MapData
        {
            get
            {
                switch (this.DisplayMode)
                {
                    case MapViewMode.Terrain:
                        return this.Map.Tiles;
                    case MapViewMode.Temperature:
                        return this.Map.Temperature;
                    case MapViewMode.Rainfall:
                        return this.Map.Rainfall;
                    case MapViewMode.Drainage:
                        return this.Map.Drainage;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        /// <summary>
        /// Construct new map view instance.
        /// </summary>
        public MapView(Position position, Size dimensions, Map map) : base(position, dimensions)
        {
            this.Map = map;
            this.Recenter();
        }
        
        /// <summary>
        /// Construct new map view instance without any map data associated with it.
        /// </summary>
        public MapView(Position position, Size dimensions) : base(position, dimensions)
        {
        }

        /// <summary>
        /// Centers the view to the center of the map
        /// </summary>
        public void Recenter()
        {
            this.CursorPosition = new Rectangle(this.MapDimensions).Center;
        }

        /// <summary>
        /// Replace map associated with this view, and reset the view state.
        /// </summary>
        public void ReplaceMap(Map map)
        {
            this.Map = map;
            this.DisplayMode = MapViewMode.Terrain;
            this.Recenter();
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
        /// For detailed maps, collect all world sites
        /// </summary>
        protected Dictionary<Position, IWorldSite> CollectSites()
        {
            var result = new Dictionary<Position, IWorldSite>();
            var map = this.Map as DetailedMap;

            foreach (var province in map.Provinces)
            {
                foreach (var city in province.AssociatedCities)
                {
                    result.Add(city.Position, city);

                    foreach (var village in city.AssociatedVillages)
                    {
                        result.Add(village.Position, village);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Perform drawing for detailed map
        /// </summary>
        protected void DrawDetailed(Surface surface, Position topLeft, Position screenPosition, Position mapPosition)
        {
            var map = this.Map as DetailedMap;
            var sites = this.CollectSites();
            var mapData = this.MapData;

            if (sites.ContainsKey(mapPosition))
            {
                var site = sites[mapPosition];
                surface.SetTile(screenPosition, site.Tile);

                // Draw title if cursor is some distance away from site
                var distance = Position.GetDistance(mapPosition, this.CursorPosition);

                if (site.ShowName &&
                    !string.IsNullOrEmpty(site.Name) &&
                    distance >= 5.0f)
                {
                    surface.DrawStringCentered(
                        new Position(screenPosition.X, screenPosition.Y - 2),
                        site.Name,
                        DefaultColors.Black,
                        UiColors.MapLabel
                        
                    );

                    var half = (int) (site.Name.Length / 2);
                    for (var ix = 0; ix < site.Name.Length; ++ix)
                    {
                        var pos = new Position((screenPosition.X + ix + 1) - half, screenPosition.Y - 1);
                        surface.SetUiShadow(pos, true);
                    }
                }
            }
            else if (this.DisplayMode == MapViewMode.Terrain
                && this.ShowResources
                && map.Resources.ContainsKey(mapPosition))
            {
                var tile = map.Resources[mapPosition].Tile;
                surface.SetTile(screenPosition, tile);
            }
            else
            {
                surface.SetTile(screenPosition, mapData[mapPosition.X, mapPosition.Y]);
            }
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

            var mapData = this.MapData;
            
            for (var iy = 0; iy < this.Dimensions.Height; ++iy)
            {
                for (var ix = 0; ix < this.Dimensions.Width; ++ix)
                {
                    var localPosition = new Position(ix, iy);
                    var mapPosition = localPosition + finalTopLeft;
                    var screenPosition = localPosition + this.Position;

                    if ((mapPosition.X < this.MapDimensions.Width && mapPosition.Y < this.MapDimensions.Height)
                        && (mapPosition.X >= 0 && mapPosition.Y >= 0) 
                        && screenPosition.IsInBounds(surface.Dimensions))
                    {
                        // Special handling when map is detailed map (this is ugly!)
                        if (this.Map is DetailedMap)
                        {
                            this.DrawDetailed(surface, finalTopLeft, screenPosition, mapPosition);
                        }
                        else
                        {
                            surface.SetTile(screenPosition, mapData[mapPosition.X, mapPosition.Y]);
                        }
                    }
                }
            }

            if (this.DrawCursor && this.CursorTimer.Flag)
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

        /// <summary>
        /// Update logic
        /// </summary>
        public void Update(double deltaTime)
        {
            this.CursorTimer.Update(deltaTime);
        }
    }
}