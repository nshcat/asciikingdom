using System.Collections.Generic;
using Engine.Graphics;

namespace Game.Data
{
    /// <summary>
    /// Represents information about a single river terrain tile.
    /// </summary>
    public class RiverTileInfo
    {
        /// <summary>
        /// The river size.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// The river tile type.
        /// </summary>
        public RiverTileType Type { get; }

        /// <summary>
        /// River size threshold after which the river is regarded to be a major one
        /// </summary>
        protected static int MajorThreshold { get; } = 5;
        
        /// <summary>
        /// Tile used for unknown river tile types
        /// </summary>
        protected static Tile UnknownRiverTile { get; } = new Tile(63, DefaultColors.Black, DefaultColors.Red);
        
        /// <summary>
        /// ASCII tiles for minor rivers
        /// </summary>
        protected static Dictionary<RiverTileType, Tile> _minorRiverTiles
            = new Dictionary<RiverTileType, Tile>
            {
                [RiverTileType.Source] = new Tile(249, Color.FromHex("#005BD8")),
                [RiverTileType.Cross] = new Tile(182, Color.FromHex("#005BD8")),
                [RiverTileType.Horizontal] = new Tile(196, Color.FromHex("#005BD8")),
                [RiverTileType.Vertical] = new Tile(179, Color.FromHex("#005BD8")),
                [RiverTileType.NorthEast] = new Tile(192, Color.FromHex("#005BD8")),
                [RiverTileType.NorthWest] = new Tile(217, Color.FromHex("#005BD8")),
                [RiverTileType.SouthEast] = new Tile(218, Color.FromHex("#005BD8")),
                [RiverTileType.SouthWest] = new Tile(191, Color.FromHex("#005BD8")),
                [RiverTileType.VerticalEast] = new Tile(195, Color.FromHex("#005BD8")),
                [RiverTileType.VerticalWest] = new Tile(180, Color.FromHex("#005BD8")),
                [RiverTileType.HorizontalNorth] = new Tile(193, Color.FromHex("#005BD8")),
                [RiverTileType.HorizontalSouth] = new Tile(194, Color.FromHex("#005BD8"))
            };
        
        /// <summary>
        /// ASCII tiles for major rivers
        /// </summary>
        protected static Dictionary<RiverTileType, Tile> _majorRiverTiles
            = new Dictionary<RiverTileType, Tile>
            {
                [RiverTileType.Source] = new Tile(249, Color.FromHex("#005BD8")),
                [RiverTileType.Cross] = new Tile(197, Color.FromHex("#005BD8")),
                [RiverTileType.Horizontal] = new Tile(205, Color.FromHex("#005BD8")),
                [RiverTileType.Vertical] = new Tile(186, Color.FromHex("#005BD8")),
                [RiverTileType.NorthEast] = new Tile(200, Color.FromHex("#005BD8")),
                [RiverTileType.NorthWest] = new Tile(188, Color.FromHex("#005BD8")),
                [RiverTileType.SouthEast] = new Tile(201, Color.FromHex("#005BD8")),
                [RiverTileType.SouthWest] = new Tile(187, Color.FromHex("#005BD8")),
                [RiverTileType.VerticalEast] = new Tile(204, Color.FromHex("#005BD8")),
                [RiverTileType.VerticalWest] = new Tile(185, Color.FromHex("#005BD8")),
                [RiverTileType.HorizontalNorth] = new Tile(202, Color.FromHex("#005BD8")),
                [RiverTileType.HorizontalSouth] = new Tile(203, Color.FromHex("#005BD8"))
            };

        /// <summary>
        /// Create a new river tile info instance
        /// </summary>
        public RiverTileInfo(RiverTileType type, int size = 1)
        {
            this.Type = type;
            this.Size = size;
        }

        /// <summary>
        /// Retrieve ASCII tile that represents this river tile
        /// </summary>
        public Tile GetTile()
        {
            if (this.Type == RiverTileType.None)
                return UnknownRiverTile;
            
            if (this.Size >= MajorThreshold)
            {
                return _majorRiverTiles[this.Type];
            }
            else
            {
                return _minorRiverTiles[this.Type];
            }
        }
    }
}