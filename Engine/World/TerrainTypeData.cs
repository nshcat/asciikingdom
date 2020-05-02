using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Graphics;

namespace Engine.World
{
    /// <summary>
    /// Represents a collection of information about a certain terrain type. Contains data such as a human-readable
    /// name and display tiles.
    /// Some terrain types have two alternative tiles, which are called primary and secondary
    /// tiles.
    /// </summary>
    public class TerrainTypeInfo
    {
        /// <summary>
        /// The primary tile.
        /// </summary>
        public Tile Primary { get; }
        
        /// <summary>
        /// The secondary tile.
        /// </summary>
        public Tile Secondary { get; }
        
        /// <summary>
        /// A human-readable name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Construct a new instance with given ASCII tiles.
        /// </summary>
        public TerrainTypeInfo(string name, Tile primary, Tile secondary)
        {
            this.Name = name;
            this.Primary = primary;
            this.Secondary = secondary;
        }

        /// <summary>
        /// Construct a new instance with both primary and secondary tiles being
        /// identical.
        /// </summary>
        public TerrainTypeInfo(string name, Tile tile) : this(name, tile, tile)
        {
            
        }
    }
    
    /// <summary>
    /// Data associated with the various terrain types
    /// </summary>
    public static class TerrainTypeData
    {
        /// <summary>
        /// Tiles used to display terrain types on the world map
        /// </summary>
        public static Dictionary<TerrainType, TerrainTypeInfo> TerrainInfo { get; }
            = new Dictionary<TerrainType, TerrainTypeInfo>
            {
                [TerrainType.MountainsLow] = new TerrainTypeInfo(
                    "Mountains",
                    new Tile(127, Color.FromHex("#808080"))
                ),
                [TerrainType.MountainsMed] = new TerrainTypeInfo(
                    "Mountains",
                    new Tile(30, Color.FromHex("#808080"))
                ),
                [TerrainType.MountainsHigh] = new TerrainTypeInfo(
                    "Mountains",
                    new Tile(30, Color.FromHex("#C0C0C0"))
                ),
                [TerrainType.MountainPeak] = new TerrainTypeInfo(
                    "Mountain Peak",
                    new Tile(94, Color.FromHex("#C0C0C0"))
                ),
                [TerrainType.Grassland] = new TerrainTypeInfo(
                    "Grassland",
                    new Tile(46, Color.FromHex("#00FF00")),
                    new Tile(252, Color.FromHex("#00FF00"))
                ),
                [TerrainType.GrasslandDry] = new TerrainTypeInfo(
                    "Dry Grassland",
                    new Tile(46, Color.FromHex("#FFFF00")),
                    new Tile(252, Color.FromHex("#FFFF00"))
                ),
                [TerrainType.Savanna] = new TerrainTypeInfo(
                    "Savanna",
                    new Tile(34, Color.FromHex("#00FF00")),
                    new Tile(252, Color.FromHex("#00FF00"))
                ),
                [TerrainType.SavannaDry] = new TerrainTypeInfo(
                    "Dry Savanna",
                    new Tile(34, Color.FromHex("#FFFF00")),
                    new Tile(252, Color.FromHex("#FFFF00"))
                ),
                [TerrainType.Shrubland] = new TerrainTypeInfo(
                    "Shrubland",
                    new Tile(34, Color.FromHex("#00FF00")),
                    new Tile(231, Color.FromHex("#00FF00"))
                ),
                [TerrainType.ShrublandDry] = new TerrainTypeInfo(
                    "Dry Shrubland",
                    new Tile(34, Color.FromHex("#FFFF00")),
                    new Tile(231, Color.FromHex("#FFFF00"))
                ),
                [TerrainType.SandDesert] = new TerrainTypeInfo(
                    "Desert",
                    new Tile(247, Color.FromHex("#FFFF00")),
                    new Tile(126, Color.FromHex("#FFFF00"))
                ),
                [TerrainType.RockyWasteland] = new TerrainTypeInfo(
                    "Rocky Wasteland",
                    new Tile(44, Color.FromHex("#C0C0C0")),
                    new Tile(39, Color.FromHex("#717100"))
                ),
                [TerrainType.Hills] = new TerrainTypeInfo(
                    "Hills",
                    new Tile(252, Color.FromHex("#00FF00")),
                    new Tile(239, Color.FromHex("#00FF00"))
                ),
                [TerrainType.HillsDry] = new TerrainTypeInfo(
                    "Dry Hills",
                    new Tile(252, Color.FromHex("#FFFF00")),
                    new Tile(239, Color.FromHex("#FFFF00"))
                ),
                [TerrainType.Ocean] = new TerrainTypeInfo(
                    "Ocean",
                    new Tile(247, Color.FromHex("#005BD8"))
                ),
                [TerrainType.Lake] = new TerrainTypeInfo(
                    "Lake",
                    new Tile(126, Color.FromHex("#458DF1"))
                ),
                [TerrainType.Tundra] = new TerrainTypeInfo(
                    "Tundra",
                    new Tile(46, Color.FromHex("#00FFFF"))
                ),
                [TerrainType.Glacier] = new TerrainTypeInfo(
                    "Glacier",
                    new Tile(176, Color.FromHex("#00FFFF")),
                    new Tile(176, DefaultColors.Black, Color.FromHex("#00FFFF"))
                ),
                [TerrainType.ConiferousForest] = new TerrainTypeInfo(
                    "Coniferous Forest",
                    new Tile(24, Color.FromHex("#008000")),
                    new Tile(23, Color.FromHex("#008000"))
                ),
                [TerrainType.TemperateBroadleafForest] = new TerrainTypeInfo(
                    "Temperate Broadleaf Forest",
                    new Tile(5, Color.FromHex("#008000")),
                    new Tile(6, Color.FromHex("#008000"))
                ),
                [TerrainType.TropicalBroadleafForest] = new TerrainTypeInfo(
                    "Tropical Broadleaf Forest",
                    new Tile(226, Color.FromHex("#008000")),
                    new Tile(6, Color.FromHex("#008000"))
                ),
                [TerrainType.Swamp] = new TerrainTypeInfo(
                    "Swamp",
                    new Tile(34, Color.FromHex("#008000")),
                    new Tile(244, Color.FromHex("#008000"))
                ),
                [TerrainType.Marsh] = new TerrainTypeInfo(
                    "Marsh",
                    new Tile(34, Color.FromHex("#008000")),
                    new Tile(252, Color.FromHex("#008000"))
                )
            };
        
        /// <summary>
        /// Fallback terrain used for unknown terrain types
        /// </summary>
        public static TerrainTypeInfo UnknownTerrain { get; } = new TerrainTypeInfo("Unknown Terrain", new Tile(63, DefaultColors.Black, DefaultColors.Magenta));

        /// <summary>
        /// Safely retrieves info for given terrain type. If no tile is available, the <see cref="UnknownTile"/> value
        /// is returned.
        /// </summary>
        public static TerrainTypeInfo GetInfo(TerrainType terrainType)
        {
            if (TerrainInfo.ContainsKey(terrainType))
                return TerrainInfo[terrainType];
            else return UnknownTerrain;
        }
    }
}