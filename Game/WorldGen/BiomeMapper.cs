using System;
using Engine.Core;
using Game.Data;
using Game.Maths;
using OpenToolkit.Graphics.OpenGL;
using Range = Game.Maths.Range;

namespace Game.WorldGen
{
    /// <summary>
    /// Class that maps elevation, temperature, drainage and rainfall to biome types
    /// </summary>
    public class BiomeMapper
    {
        /// <summary>
        /// The two rough climate zones of habitable land
        /// </summary>
        private enum ClimateZone
        {
            Temperate,
            Tropical
        }
        
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
        /// The world seed
        /// </summary>
        protected int Seed { get; set; }
        
        /// <summary>
        /// Construct a new biome mapper instance.
        /// </summary>
        public BiomeMapper(Size dimensions, int seed, HeightMap elevation, RainfallMap rainfall, DrainageMap drainage,
            TemperatureMap temperature)
        {
            this.Seed = seed;
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
            var rng = new Random(this.Seed + 13185);
            
            // Chance ranges for broad leaf forest generation, pulled out of the loops to avoid repeated heap allocations
            //var sourceRange = new Range(this.Temperature.ColderThreshold, this.Temperature.ColdThreshold);
            // Alternative: Source range starting at 0.0f. Causes forests to almost never be 100% coniferous
            var coniferousSrcRange = new Range(this.Temperature.ColdestThreshold, this.Temperature.ColdThreshold);
            var destRange = new Range(0.0f, 1.0f);

            var difference = this.Temperature.WarmThreshold - this.Temperature.ColdThreshold;
            var jungleSrcRange = new Range(this.Temperature.ColdThreshold + (difference / 2), this.Temperature.WarmThreshold);
            var climateZoneSrcRange = jungleSrcRange;
            
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    var elevation = this.Elevation.HeightLevels[ix, iy];
                    var drainage = this.Drainage[ix, iy];
                    var temperature = this.Temperature.TemperatureLevels[ix, iy];
                    var rawTemperature = this.Temperature[ix, iy];
                    var rainfall = this.Rainfall[ix, iy];
                    
                    // Determine rough climate zone.
                    var climateZone = ClimateZone.Temperate;

                    // Warmer and warmest temperatures are always tropical
                    if (temperature == TemperatureLevel.Warmer ||
                        temperature == TemperatureLevel.Warmest)
                    {
                        climateZone = ClimateZone.Tropical;
                    }
                    else if(temperature == TemperatureLevel.Warm) // Smoothly transition in warm temperature zone
                    {
                        var tropicalChance = MathUtil.Map(rawTemperature, climateZoneSrcRange, destRange);
                        if (rng.NextDouble() <= tropicalChance)
                            climateZone = ClimateZone.Tropical;
                    }

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
                                if (climateZone == ClimateZone.Tropical)
                                {
                                    // Tropical zones have little bad lands, but lots of deserts and rocky
                                    // wastelands
                                    if (drainage < 0.85f)
                                        type = TerrainType.SandDesert;
                                    else
                                        type = TerrainType.RockyWasteland;
                                }
                                else
                                {
                                    // Temperate regions have no deserts
                                    if (drainage < 0.65f)
                                        type = TerrainType.RockyWasteland;
                                    else
                                    {
                                        type = TerrainType.BadLands;
                                    }
                                }
                            }
                            else if (rainfall < 0.66f)
                            {
                                if (climateZone == ClimateZone.Tropical)
                                {
                                    // Since evaporation is high, even moderately high rainfall levels will still result
                                    // in arid biomes
                                    if (rainfall < 0.30f)
                                    {
                                        type = TerrainType.SandDesert;
                                    }
                                    else if (rainfall < 0.45f)
                                    {
                                        if (drainage < 0.5)
                                            type = TerrainType.Steppe;
                                        else
                                            type = TerrainType.HillySteppe;
                                    }
                                    else if (rainfall < 0.55f)
                                    {
                                        if (drainage < 0.5)
                                            type = TerrainType.SavannaDry;
                                        else
                                            type = TerrainType.HillsDry;
                                    }
                                    else
                                    {
                                        if (drainage < 0.5)
                                            type = TerrainType.Savanna;
                                        else
                                            type = TerrainType.HillySavanna;
                                    }
                                }
                                else // Temperate climate
                                {
                                    // In a temperate climate, evaporation is low. Thus, only areas with very little
                                    // rainfall will develop arid biomes
                                    if (rainfall < 0.20f)
                                    {
                                        if (drainage < 0.5)
                                            type = TerrainType.ShrublandDry;
                                        else
                                            type = TerrainType.HillsDry;
                                    }
                                    else if (rainfall < 0.40f)
                                    {
                                        if (drainage < 0.5)
                                            type = TerrainType.Shrubland;
                                        else
                                            type = TerrainType.Hills;
                                    }
                                    else if(rainfall > 0.55 && drainage < 0.15f)
                                    {
                                        type = TerrainType.Marsh;
                                    }
                                    else
                                    {
                                        if (drainage < 0.5)
                                            type = TerrainType.Grassland;
                                        else
                                            type = TerrainType.HillyGrassland;
                                    }
                                }
                            }
                            // Grasslands region
                            /*else if (rainfall < 0.2f)
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
                                if (drainage < 0.12f)
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
                            }*/
                            // Forest/Swamp region
                            else
                            {
                                if (drainage < 0.12f/*0.10f*/)
                                    type = TerrainType.Swamp;
                                else
                                {
                                    if (temperature == TemperatureLevel.Warmer ||
                                        temperature == TemperatureLevel.Warmest)
                                        type = TerrainType.TropicalBroadleafForest;
                                    else if (temperature == TemperatureLevel.Warm)
                                    {
                                        var jungleChance = MathUtil.Map(rawTemperature, jungleSrcRange, destRange);
                                        if (rng.NextDouble() <= jungleChance)
                                            type = TerrainType.TropicalBroadleafForest;
                                        else
                                            type = TerrainType.TemperateBroadleafForest;
                                    }
                                    else
                                    {
                                        // In colder climates, we use a scaling chance of generation broad leaf forests
                                        // to smoothly transition from temperate zones with broad leaf forests to
                                        // more colder zones with coniferous forests
                                        var broadleafChance = MathUtil.Map(rawTemperature, coniferousSrcRange, destRange);

                                       // broadleafChance *= broadleafChance;
                                        
                                        if (rng.NextDouble() <= broadleafChance)
                                            type = TerrainType.TemperateBroadleafForest;
                                        else
                                            type = TerrainType.ConiferousForest;
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