namespace Game.WorldGen
{
    /// <summary>
    /// Parameters used during world generation.
    /// </summary>
    public class WorldParameters
    {
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
        /// The percentage amount of terrain that is supposed to be underwater, in percent
        /// </summary>
        /// <remarks>
        /// This is used to determine the ocean water level
        /// </remarks>
        public float UnderWaterPercentage { get; set; } = 0.65f;//0.55f;

        /// <summary>
        /// The percentage amount of terrain that is supposed to be under the tree line
        /// </summary>
        /// <remarks>
        /// The tree line is the border between normal land and mountains
        /// </remarks>
        public float TreeLinePercentage { get; set; } = 0.945f; // 0.935f;

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
    }
}