using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            Color.FromHex("#A59E01"),
            Color.FromHex("#908B22"),
            Color.FromHex("#C9DA00"),
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
            Color.FromHex("#C8CC47"),
            Color.FromHex("#C1C801"),
            Color.FromHex("#C9DA00"),
            Color.FromHex("#B2BC44"),
            Color.FromHex("#CDC300"),
            Color.FromHex("#A8A10C"),
            Color.FromHex("#D7D03D"),
            Color.FromHex("#8ACD38"),
            Color.FromHex("#A6CD38"),
            Color.FromHex("#C8CD38"),
            Color.FromHex("#A5AB04"),
            Color.FromHex("#7EA025"),
            Color.FromHex("#8DA44E"),
            Color.FromHex("#A1A44E")
        };

        private static List<Color> HillsPalette = new List<Color>
        {
            Color.FromHex("#87B63E"),
            Color.FromHex("#ACDF5E"),
            Color.FromHex("#C7DF5E"),
            Color.FromHex("#A5C908"),
            Color.FromHex("#93B600"),
            Color.FromHex("#B8C809"),
            Color.FromHex("#95D44C")
        };
        
        private static List<Color> DryHillsPalette = new List<Color>
        {
            Color.FromHex("#D1D85A"),
            Color.FromHex("#C1C66A"),
            Color.FromHex("#C5CA73"),
            Color.FromHex("#C3AB0C"),
            Color.FromHex("#B7C700"),
            Color.FromHex("#CFDB4A"),
            Color.FromHex("#9ABF4A"),
            Color.FromHex("#C9C956"),
            Color.FromHex("#C4CA48"),
            Color.FromHex("#BBE840"),
            Color.FromHex("#D8E527")
        };

        /// <summary>
        /// Color palette for tropical shrubland such as savannas etc
        /// </summary>
        private static List<Color> SavannaPalette = new List<Color>
        {
            Color.FromHex("#EAEDA2"),
            Color.FromHex("#E9F058"),
            Color.FromHex("#D9DF63"),
            Color.FromHex("#FADE34"),
            Color.FromHex("#CAC645"),
            Color.FromHex("#E6D672"),
            Color.FromHex("#E6DB93"),
            Color.FromHex("#F2D885"),
            Color.FromHex("#FFD85A"),
            Color.FromHex("#FFD652"),
            Color.FromHex("#FFD652"),
        };
        
        private static List<Color> SavannaTreePalette = new List<Color>
        {
            Color.FromHex("#73B94C"),
            Color.FromHex("#9CD320"),
            Color.FromHex("#5AD500"),
            Color.FromHex("#96C970"),
            Color.FromHex("#8CD654"),
            Color.FromHex("#6CBB33"),
            Color.FromHex("#55B80E"),
            Color.FromHex("#89C95A"),
            Color.FromHex("#95CC6E"),
            Color.FromHex("#7CB058"),
            Color.FromHex("#7B9E63")
        };

        /// <summary>
        /// Set of terrain types that do not allow sites
        /// </summary>
        private static HashSet<TerrainType> ForbidsSites = new HashSet<TerrainType>
        {
            TerrainType.Glacier,
            TerrainType.Lake,
            TerrainType.Marsh,
            TerrainType.Ocean,
            TerrainType.River,
            TerrainType.Swamp,
            TerrainType.Unknown,
            TerrainType.MountainPeak,
            TerrainType.MountainsHigh,
            TerrainType.MountainsMed,
            TerrainType.MountainsLow,
            TerrainType.SeaIce
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
                    WithPalette(
                        new WeightedCollection<int>
                        {
                            { 1.0, 34 },
                            { 1.0, 252 }
                        }, 
                        SavannaPalette
                    ).Append(
                        WithPalette(
                            new WeightedCollection<int>
                            {
                                { 0.50, 231 },
                                { 0.15, 34 }
                            }, 
                            SavannaTreePalette
                        )
                    )
                ),
                [TerrainType.SavannaDry] = new TerrainTypeInfo(
                    "Dry Savanna",
                    WithPalette(new WeightedCollection<int>(34, 252), SavannaPalette)
                ),
                [TerrainType.HillySavanna] = new TerrainTypeInfo(
                    "Hilly Savanna",
                    WithPalette(new WeightedCollection<int>(34, 239), SavannaPalette)
                ),
                [TerrainType.Steppe] = new TerrainTypeInfo(
                    "Steppe",
                    WithPalette(new WeightedCollection<int>(44, 34), SavannaPalette)
                ),
                [TerrainType.HillySteppe] = new TerrainTypeInfo(
                    "Hilly Steppe",
                    WithPalette(new WeightedCollection<int>(44, 239), SavannaPalette)
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
                                { 0.05, 231 },
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
                    WithPalette(new WeightedCollection<int>(252, 239), HillsPalette)
                ),
                [TerrainType.HillsDry] = new TerrainTypeInfo(
                    "Dry Hills",
                    WithPalette(new WeightedCollection<int>(252, 239), DryHillsPalette)
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
                    WithPalette(new WeightedCollection<int>(34, 244), DarkGreenPalette)
                ),
                [TerrainType.Marsh] = new TerrainTypeInfo(
                    "Marsh",
                    WithPalette(new WeightedCollection<int>(34, 252), DarkGreenPalette)
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
        /// Checks whether given terrain type accepts sites built on them
        /// </summary>
        public static bool AcceptsSites(TerrainType terrainType)
        {
            return !ForbidsSites.Contains(terrainType);
        }

        /// <summary>
        /// Checks whether given terrain type accepts crops
        /// </summary>
        public static bool AcceptsCrops(TerrainType terrainType)
        {
            return AcceptsSites(terrainType);
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