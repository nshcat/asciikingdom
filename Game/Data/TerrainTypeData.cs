using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using Engine.Graphics;
using Game.Core;

namespace Game.Data
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
        /// Weighted collection of all tiles that are used to display this terrain type.
        /// </summary>
        public WeightedCollection<Tile> Tiles { get; }
        
        /// <summary>
        /// A human-readable name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Construct a new instance with given ASCII tiles.
        /// </summary>
        public TerrainTypeInfo(string name, WeightedCollection<Tile> tiles)
        {
            this.Name = name;
            this.Tiles = tiles;
        }

        /// <summary>
        /// Construct a new instance with a single tile variant.
        /// </summary>
        public TerrainTypeInfo(string name, Tile tile)
            : this(name, new WeightedCollection<Tile> {{1.0, tile}})
        {
            
        }
        
        /// <summary>
        /// Construct a new instance with a two possible tile variants.
        /// </summary>
        public TerrainTypeInfo(string name, Tile primary, Tile secondary)
            : this(name, new WeightedCollection<Tile> {{0.5, primary}, {0.5, secondary}})
        {
            
        }

        /// <summary>
        /// Pick a random tile for this terrain type.
        /// </summary>
        public Tile PickTile(Random rng)
        {
            return this.Tiles.Next(rng);
        }
    }
    
    /// <summary>
    /// Data associated with the various terrain types
    /// </summary>
    public static class TerrainTypeData
    {
        /// <summary>
        /// Green palette, used for temperate broadleaf forests
        /// </summary>
        private static List<Color> GreenPalette = new List<Color>
        {
            Color.FromHex("#00CC55"),
            Color.FromHex("#05AE4C"),
            Color.FromHex("#049A42"),
            Color.FromHex("#039A56"),
            Color.FromHex("#06AC4B"),
            Color.FromHex("#299D59"),
            Color.FromHex("#00AD47")
        };

        /// <summary>
        /// Dark green palette, used for coniferous forests
        /// </summary>
        private static List<Color> DarkGreenPalette = new List<Color>
        {
            Color.FromHex("#0C7A39"),
            Color.FromHex("#00913B"),
            Color.FromHex("#046E2F"),
            Color.FromHex("#257F4A"),
            Color.FromHex("#20663D"),
            Color.FromHex("#1B7F45")
        };

        /// <summary>
        /// Lush green palette, used for tropical forests and marshes/swamps
        /// </summary>
        private static List<Color> TropicalGreenPalette = new List<Color>
        {
            Color.FromHex("#08B727"),
            Color.FromHex("#00B63D"),
            Color.FromHex("#008D2F"),
            Color.FromHex("#06B504"),
            Color.FromHex("#30A52F"),
            Color.FromHex("#01AB00"),
            Color.FromHex("#00781A")
        };
        
        /// <summary>
        /// Sand color palette
        /// </summary>
        private static List<Color> SandPalette = new List<Color>
        {
            Color.FromHex("#E7E78A"),
            Color.FromHex("#E0E072"),
            Color.FromHex("#E5E570"),
            Color.FromHex("#D5D56E"),
            Color.FromHex("#DCDC8B"),
            Color.FromHex("#ECEC84")
        };

        /// <summary>
        /// Color palette for grasslands
        /// </summary>
        private static List<Color> GrasslandPalette = new List<Color>
        {
            Color.FromHex("#5EC45B"),
            Color.FromHex("#5AA158"),
            Color.FromHex("#32BA47"),
            Color.FromHex("#2CBA4D"),
            Color.FromHex("#4EBA68"),
            Color.FromHex("#0BD448"),
            Color.FromHex("#06A837"),
            Color.FromHex("#66C24E")
        };

        /// <summary>
        /// Color palette for dry grasslands
        /// </summary>
        private static List<Color> DryGrasslandPalette = new List<Color>
        {
            Color.FromHex("#9FC34C"),
            Color.FromHex("#70C34C"),
            Color.FromHex("#6DCE42"),
            Color.FromHex("#8CD32C"),
            Color.FromHex("#A0C623"),
            Color.FromHex("#44C43A"),
            Color.FromHex("#99C43A")
        };
        
        /// <summary>
        /// Color palette for dry shrubland
        /// </summary>
        private static List<Color> ShrublandPalette = new List<Color>
        {
            Color.FromHex("#ACBA12"),
            Color.FromHex("#73BA12"),
            Color.FromHex("#8CC341"),
            Color.FromHex("#97B33B"),
            Color.FromHex("#70B62D"),
            Color.FromHex("#81BE0C"),
            Color.FromHex("#9BCE53"),
            Color.FromHex("#B3BA3F"),
            Color.FromHex("#9BC435")
        };
        
        private static List<Color> DryShrublandPalette = new List<Color>
        {
            Color.FromHex("#B3BA12"),
            Color.FromHex("#949B0E"),
            Color.FromHex("#BFBE00"),
            Color.FromHex("#8ACD38"),
            Color.FromHex("#A6CD38"),
            Color.FromHex("#C8CD38"),
            Color.FromHex("#A5AB04"),
            Color.FromHex("#7EA025"),
            Color.FromHex("#8DA44E"),
            Color.FromHex("#A1A44E")
        };

        /// <summary>
        /// Color palette for tropical shrubland such as savannas etc
        /// </summary>
        private static List<Color> SavannaPalette = new List<Color>
        {
            
        };

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
                    WithPalette(
                        new WeightedCollection<int>
                        {
                            { 0.65, 46 },
                            { 1.0, 252 },
                            { 0.15, 34 }
                        },
                        GrasslandPalette
                    )
                ),
                [TerrainType.HillyGrassland] = new TerrainTypeInfo(
                    "Hilly Grassland",
                    WithPalette(
                        new WeightedCollection<int>
                        {
                            { 1.0, 239 },
                            { 0.35, 46 },
                            { 0.5, 252 },
                            { 0.15, 34 }
                        },
                        GrasslandPalette
                    )
                ),
                [TerrainType.BadLands] = new TerrainTypeInfo(
                    "Badlands",
                    new Tile(86, Color.FromHex("#804B09")),
                    new Tile(251, Color.FromHex("#804B09"))
                ),
                [TerrainType.GrasslandDry] = new TerrainTypeInfo(
                    "Dry Grassland",
                    WithPalette(
                        new WeightedCollection<int>
                        {
                            { 0.65, 46 },
                            { 1.0, 252 },
                            { 0.15, 34 }
                        },
                        DryGrasslandPalette
                    )
                ),
                [TerrainType.Savanna] = new TerrainTypeInfo(
                    "Savanna",
                    new Tile(34, Color.FromHex("#00FF00")),
                    new Tile(252, Color.FromHex("#00FF00"))
                ),
                [TerrainType.HillySavanna] = new TerrainTypeInfo(
                    "Hilly Savanna",
                    new Tile(34, Color.FromHex("#00FF00")),
                    new Tile(239, Color.FromHex("#00FF00"))
                ),
                [TerrainType.Steppe] = new TerrainTypeInfo(
                    "Steppe",
                    new Tile(44, Color.FromHex("#00FF00")),
                    new Tile(34, Color.FromHex("#00FF00"))
                ),
                [TerrainType.HillySteppe] = new TerrainTypeInfo(
                    "Hilly Steppe",
                    new Tile(44, Color.FromHex("#00FF00")),
                    new Tile(239, Color.FromHex("#00FF00"))
                ),
                [TerrainType.SavannaDry] = new TerrainTypeInfo(
                    "Dry Savanna",
                    new Tile(34, Color.FromHex("#FFFF00")),
                    new Tile(252, Color.FromHex("#FFFF00"))
                ),
                [TerrainType.Shrubland] = new TerrainTypeInfo(
                    "Shrubland",
                    WithPalette(
                        new WeightedCollection<int>
                        {
                            { 0.1, 252 },
                            { 0.75, 231 },
                            { 1.0, 34 }
                        },
                        ShrublandPalette
                    ).Append(
                        WithPalette(
                            new WeightedCollection<int>
                            {
                                { 0.25, 231 },
                                { 0.1, 34 }
                            },
                            DryGrasslandPalette
                        )
                    )
                ),
                [TerrainType.HillyShrubland] = new TerrainTypeInfo(
                    "Hilly Shrubland",
                    new Tile(239, Color.FromHex("#00FF00")),
                    new Tile(231, Color.FromHex("#00FF00"))
                ),
                [TerrainType.ShrublandDry] = new TerrainTypeInfo(
                    "Dry Shrubland",
                    WithPalette(
                        new WeightedCollection<int>
                        {
                            { 0.1, 252 },
                            { 0.75, 231 },
                            { 1.0, 34 }
                        },
                        DryShrublandPalette
                    ).Append(
                        WithPalette(
                            new WeightedCollection<int>
                            {
                                { 0.1, 231 },
                                { 0.05, 34 }
                            },
                            ShrublandPalette
                        )
                    )
                ),
                [TerrainType.SandDesert] = new TerrainTypeInfo(
                    "Desert",
                    WithPalette(new WeightedCollection<int>(247, 126), SandPalette)
                ),
                [TerrainType.RockyWasteland] = new TerrainTypeInfo(
                    "Rocky Wasteland",
                    new Tile(44, Color.FromHex("#C0C0C0")),
                    new Tile(39, Color.FromHex("#804B09"))
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
                [TerrainType.River] = new TerrainTypeInfo(
                    "River",
                    new Tile(197, Color.FromHex("#005BD8"))
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
                [TerrainType.SeaIce] = new TerrainTypeInfo(
                    "Sea Ice",
                    new Tile(176, Color.FromHex("#00FFFF")),
                    new Tile(247, Color.FromHex("#005BD8"))
                ),
                [TerrainType.ConiferousForest] = new TerrainTypeInfo(
                    "Coniferous Forest",
                    WithPalette(new WeightedCollection<int>(24, 23), DarkGreenPalette)
                ),
                [TerrainType.TemperateBroadleafForest] = new TerrainTypeInfo(
                    "Temperate Broadleaf Forest",
                    WithPalette(new WeightedCollection<int>(5, 6), GreenPalette)
                ),
                [TerrainType.TropicalBroadleafForest] = new TerrainTypeInfo(
                    "Tropical Broadleaf Forest",
                    WithPalette(new WeightedCollection<int>(226, 6), TropicalGreenPalette)
                ),
                [TerrainType.Swamp] = new TerrainTypeInfo(
                    "Swamp",
                    WithPalette(new WeightedCollection<int>(34, 244), TropicalGreenPalette)
                ),
                [TerrainType.Marsh] = new TerrainTypeInfo(
                    "Marsh",
                    WithPalette(new WeightedCollection<int>(34, 252), TropicalGreenPalette)
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

        /// <summary>
        /// Create a new weighted collection based on given weighted collection of ASCII glyphs, where each glyph
        /// is present in any of the colors in the palette.
        /// </summary>
        /// <remarks>
        /// This method adjusts the weights in order to make each symbol equally as likely as before
        /// </remarks>
        private static WeightedCollection<Tile> WithPalette(
            WeightedCollection<int> glyphs,
            IEnumerable<Color> palette)
        {
            var collection = new WeightedCollection<Tile>();
            var paletteSize = palette.Count();

            foreach (var entry in glyphs)
            {
                var weight = entry.Weight / (float) paletteSize;

                foreach (var color in palette)
                {
                    collection.Add(weight, new Tile(entry.Value, color, DefaultColors.Black));
                }
            }

            return collection;
        }
    }
}