using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    /// A scene component that allows rendering of a world map
    /// </summary>
    public class MapView : GameView
    {
        /// <summary>
        /// The map to display
        /// </summary>
        public Map Map { get; set; }

        /// <summary>
        /// What map component to display
        /// </summary>
        public MapViewMode DisplayMode { get; set; } = MapViewMode.Terrain;

        /// <summary>
        /// Whether this view currently is associated with any world data
        /// </summary>
        public bool HasMapData => this.Map != null;

        /// <summary>
        /// Whether to show special resources on the map
        /// </summary>
        public bool ShowResources { get; set; } = true;

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
        public MapView(Position position, Size dimensions, Map map) : base(position, dimensions, map.Dimensions)
        {
            this.Map = map;
        }
        
        /// <summary>
        /// Construct new map view instance without any map data associated with it.
        /// </summary>
        public MapView(Position position, Size dimensions) : base(position, dimensions, Size.Empty)
        {
        }

        /// <summary>
        /// Replace map associated with this view, and reset the view state.
        /// </summary>
        public void ReplaceMap(Map map)
        {
            this.Map = map;
            this.DisplayMode = MapViewMode.Terrain;
            this.WorldDimensions = map.Dimensions;
            this.Recenter();
        }

        /// <summary>
        /// Render detailed map cell
        /// </summary>
        protected override void RenderCell(Surface surface, Position worldPosition, Position screenPosition)
        {
            if (this.Map is DetailedMap detailedMap
                && this.DisplayMode == MapViewMode.Terrain
                && this.ShowResources
                && detailedMap.Resources.ContainsKey(worldPosition))
            {
                var tile = detailedMap.Resources[worldPosition].Tile;
                surface.SetTile(screenPosition, tile);
            }
            else
            {
                surface.SetTile(screenPosition, this.MapData[worldPosition.X, worldPosition.Y]);
            }
        }

        /// <summary>
        /// The map view is rendered when there is map data associated with it.
        /// </summary>
        protected override bool ShouldRender() => this.HasMapData;
    }
}