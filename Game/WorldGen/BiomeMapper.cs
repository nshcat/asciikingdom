using Engine.Core;
using Game.Data;
using OpenToolkit.Graphics.OpenGL;

namespace Game.WorldGen
{
    /// <summary>
    /// Class that maps elevation, temperature, drainage and rainfall to biome types
    /// </summary>
    public class BiomeMapper
    {
        /// <summary>
        /// Terrain type layer
        /// </summary>
        public TerrainType[,] TerrainTypes { get; protected set; }
        
        /// <summary>
        /// The layer dimensions
        /// </summary>
        public Size Dimensions { get; protected set; }
        
        /// <summary>
        /// Elevation layer
        /// </summary>
        protected HeightMap Elevation { get; set; }
        
        /// <summary>
        /// Rainfall layer
        /// </summary>
        protected RainfallMap Rainfall { get; set; }
        
        /// <summary>
        /// Drainage layer
        /// </summary>
        protected DrainageMap Drainage { get; set; }
        
        /// <summary>
        /// Temperature layer
        /// </summary>
        protected TemperatureMap Temperature { get; set; }

        /// <summary>
        /// Construct a new biome mapper instance.
        /// </summary>
        public BiomeMapper(Size dimensions, HeightMap elevation, RainfallMap rainfall, DrainageMap drainage,
            TemperatureMap temperature)
        {
            this.Dimensions = dimensions;
            this.Elevation = elevation;
            this.Rainfall = rainfall;
            this.Drainage = drainage;
            this.Temperature = temperature;
            
            this.TerrainTypes = new TerrainType[dimensions.Width, dimensions.Height];

            this.MapBiomes();
        }

        /// <summary>
        /// Map all biomes.
        /// </summary>
        protected void MapBiomes()
        {
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var elevation = this.Elevation.HeightLevels[ix, iy];
                    var drainage = this.Drainage[ix, iy];
                    var temperature = this.Temperature.TemperatureLevels[ix, iy];
                    var rainfall = this.Rainfall[ix, iy];

                    var type = TerrainType.Unknown;

                    if (elevation == HeightLevel.Sea)
                    {
                        if (temperature == TemperatureLevel.Coldest)
                            type = TerrainType.Glacier;
                        else if (temperature == TemperatureLevel.Colder)
                            type = TerrainType.SeaIce;
                        else
                            type = TerrainType.Ocean;
                    }
                    else if(elevation == HeightLevel.Land)
                    {
                        if (temperature == TemperatureLevel.Coldest)
                            type = TerrainType.Glacier;
                        else if (temperature == TemperatureLevel.Colder)
                            type = TerrainType.Tundra;
                        else
                        {
                            // Very dry region
                            if (rainfall < 0.1f)
                            {
                                if (drainage < 0.33f)
                                    type = TerrainType.SandDesert;
                                else if (drainage < 0.5f)
                                    type = TerrainType.RockyWasteland;
                                else
                                {
                                    type = TerrainType.BadLands;
                                }
                            }
                            // Grasslands region
                            else if (rainfall < 0.2f)
                            {
                                if (drainage < 0.5f)
                                {
                                    type = TerrainType.Grassland;
                                }
                                else
                                {
                                    type = TerrainType.Hills;
                                }
                            }
                            // Savanna region
                            else if (rainfall < 0.33f)
                            {
                                if (drainage < 0.5f)
                                {
                                    type = TerrainType.Savanna;
                                }
                                else
                                {
                                    type = TerrainType.HillsDry;
                                }
                            }
                            // Marsh, shrubland region
                            else if (rainfall < 0.66f)
                            {
                                if (drainage < 0.10f)
                                    type = TerrainType.Marsh;
                                else if (drainage < 0.5f)
                                {
                                    if (temperature == TemperatureLevel.Warmer ||
                                        temperature == TemperatureLevel.Warmest)
                                        type = TerrainType.ShrublandDry;
                                    else
                                        type = TerrainType.Shrubland;
                                }
                                else
                                {
                                    if (temperature == TemperatureLevel.Warmer ||
                                        temperature == TemperatureLevel.Warmest)
                                        type = TerrainType.HillsDry;
                                    else
                                        type = TerrainType.Hills;
                                }
                            }
                            // Forest/Swamp region
                            else
                            {
                                if (drainage < 0.10f)
                                    type = TerrainType.Swamp;
                                else
                                {
                                    // Its a forest. Determine type
                                    if (rainfall < 0.75f)
                                    {
                                        type = TerrainType.ConiferousForest;
                                    }
                                    else // Broadleaf forest. Which type?
                                    {
                                        if (temperature == TemperatureLevel.Warmer ||
                                            temperature == TemperatureLevel.Warmest)
                                            type = TerrainType.TropicalBroadleafForest;
                                        else
                                            type = TerrainType.TemperateBroadleafForest;
                                    }
                                }
                            }
                        }
                    }
                    else if (elevation == HeightLevel.LowMountain)
                        type = TerrainType.MountainsLow;
                    else if (elevation == HeightLevel.MediumMountain)
                        type = TerrainType.MountainsMed;
                    else if (elevation == HeightLevel.HighMountain)
                        type = TerrainType.MountainsHigh;
                    else if (elevation == HeightLevel.MountainPeak)
                        type = TerrainType.MountainPeak;

                    this.TerrainTypes[ix, iy] = type;
                }
            }
        }
    }
}