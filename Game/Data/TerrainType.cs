namespace Game.Data
{
    /// <summary>
    /// Describes the different types of terrain that can appear on the game map
    /// </summary>
    public enum TerrainType
    {
        Unknown,
        Ocean,
        MountainsLow,
        MountainsMed,
        MountainsHigh,
        MountainPeak,
        Grassland,
        GrasslandDry,
        HillyGrassland,
        HillyShrubland,
        HillySavanna,
        Savanna,
        SavannaDry,
        Steppe,
        HillySteppe,
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
        HillsDry,
        
        // This is a special marker. Metadata is used to correctly render
        // river glyphs based on orientation and interconnectivity.
        River,
        Lake
    }
}