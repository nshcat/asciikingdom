namespace Game.Data
{
    /// <summary>
    /// Describes the different types of terrain that can appear on the game map
    /// </summary>
    public enum TerrainType
    {
        Unknown,
        Ocean,
        Lake,
        MountainsLow,
        MountainsMed,
        MountainsHigh,
        MountainPeak,
        Grassland,
        GrasslandDry,
        Savanna,
        SavannaDry,
        Shrubland,
        ShrublandDry,
        SandDesert,
        RockyWasteland,
        Swamp,
        Marsh,
        Tundra,
        Glacier,
        SeaIce,
        TemperateBroadleafForest,
        TropicalBroadleafForest,
        ConiferousForest,
        BadLands,
        
        // Not sure if these will be used
        Hills,
        HillsDry
    }
}