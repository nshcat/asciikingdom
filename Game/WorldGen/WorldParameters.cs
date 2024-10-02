namespace Game.WorldGen
{
    /// <summary>
    /// Parameters used during world generation.
    /// </summary>
    public struct WorldParameters
    {
        /// <summary>
        /// Whether the depression filling algorithm should use randomized parameters
        /// </summary>
        /// <remarks>
        /// This avoids weird artifacts like very long, straight, diagonal lines from appearing, but slightly changes
        /// how worlds look.
        /// </remarks>
        public bool RandomizedSinkFilling { get; set; } = false;
        
        /// <summary>
        /// The chance that a resource will spawn on a selected tile
        /// </summary>
        public float ResourceSpawnChance { get; set; } = 0.7f;

        /// <summary>
        /// Whether the whole height map should be multiplied with itself. Makes hills
        /// steeper.
        /// </summary>
        public bool AccentuateHills { get; set; } = false;

        /// <summary>
        /// Whether the map generator should force ocean to be present at both the west and east sides of the map.
        /// This will stop land from generating right at the map border.
        /// </summary>
        public bool ForceOceanSides { get; set; } = true;

        /// <summary>
        /// Whether to prevent colder and coldest zones from appearing at longitudes more than <see cref="ColdZoneLongitudeLimit"/> percent
        /// </summary>
        public bool LimitColdZones { get; set; } = true;

        /// <summary>
        /// Percentage longitude limit after which no coldest and colder zones may appear
        /// </summary>
        public float ColdZoneLongitudeLimit { get; set; } = 0.17f;//0.15f;

        /// <summary>
        /// The percentage amount of terrain that is supposed to be underwater, in percent
        /// </summary>
        /// <remarks>
        /// This is used to determine the ocean water level
        /// 0.65f for big land masses
        /// 0.85f for lots of small islands
        /// </remarks>
        public float UnderWaterPercentage { get; set; } = 0.85f;//0.65f;//0.55f;

        /// <summary>
        /// The percentage amount of terrain that is supposed to be under the tree line
        /// </summary>
        /// <remarks>
        /// The tree line is the border between normal land and mountains
        /// 0.945f for big land masses
        /// 0.98f for lots of small islands
        /// </remarks>
        public float TreeLinePercentage { get; set; } = 0.98f; //0.945f; // 0.935f;

        /// <summary>
        /// Percentage of drainage values that are in the range [0, 0.32]
        /// </summary>
        public float DesertPercentage { get; set; } = 0.55f;

        /// <summary>
        /// Percentage of drainage values that are in range [0, 0.49]
        /// </summary>
        public float RockyPercentage { get; set; } = 0.75f;

        /// <summary>
        /// Percentage of drainage values that are in range [0, 0.65]
        /// </summary>
        public float HillsPercentage { get; set; } = 0.85f;

        /// <summary>
        /// Percentage of rainfall values that are in range [0, 9]
        /// </summary>
        public float BarrenPercentage { get; set; } = 0.15f;

        /// <summary>
        /// Percentage of rainfall values that are in range [0, 65]
        /// </summary>
        public float GrassPercentage { get; set; } = 0.75f;

        /// <summary>
        /// Percentage of rainfall values that are in range [0, 74]
        /// </summary>
        public float ConiferPercentage { get; set; } = 0.80f;

        /// <summary>
        /// Percentage of temperatures up to coldest 
        /// </summary>
        public float ColdestPercentage { get; set; } = 0.10f;

        /// <summary>
        /// Percentage of temperatures up to colder 
        /// </summary>
        public float ColderPercentage { get; set; } = 0.15f;
        
        /// <summary>
        /// Percentage of temperatures up to cold
        /// </summary>
        public float ColdPercentage { get; set; } = 0.40f;
        
        /// <summary>
        /// Percentage of temperatures up to warm
        /// </summary>
        public float WarmPercentage { get; set; } = 0.75f;
        
        /// <summary>
        /// Percentage of temperatures up to warmer
        /// </summary>
        public float WarmerPercentage { get; set; } = 0.85f;

        public WorldParameters()
        {
        }
    }
}