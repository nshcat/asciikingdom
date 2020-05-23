using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Graphics;

namespace Game.Data
{
    /// <summary>
    /// Static class containing tile data for the different <see cref="RiverTileType"/> values.
    /// </summary>
    public static class RiverTileTypeData
    {
        /// <summary>
        /// Tiles used to display the various river tile types
        /// </summary>
        public static Dictionary<RiverTileType, Tile> RiverTileInfo { get; }
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
                [RiverTileType.VerticalEast] = new Tile(180, Color.FromHex("#005BD8")),
                [RiverTileType.VerticalWest] = new Tile(195, Color.FromHex("#005BD8")),
                [RiverTileType.HorizontalNorth] = new Tile(193, Color.FromHex("#005BD8")),
                [RiverTileType.HorizontalSouth] = new Tile(194, Color.FromHex("#005BD8"))
            };

        /// <summary>
        /// Tile used for unknown river tile types
        /// </summary>
        public static Tile UnknownRiverTile { get; } = new Tile(63, DefaultColors.Black, DefaultColors.Red);

        /// <summary>
        /// Safely retrieve ASCII tile for given river tile type. Will return a special error tile
        /// when no tile is registered for the given tile type.
        /// </summary>
        public static Tile GetRiverTile(RiverTileType tileType)
        {
            if (RiverTileInfo.ContainsKey(tileType))
                return RiverTileInfo[tileType];
            else
                return UnknownRiverTile;
        }
    }
}