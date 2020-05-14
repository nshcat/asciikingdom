namespace Game.WorldGen
{
    /// <summary>
    /// Parameters used during world generation.
    /// </summary>
    public class WorldParameters
    {
        /// <summary>
        /// The amount of terrain that is supposed to be underwater, in percent
        /// </summary>
        /// <remarks>
        /// This is used to determine the ocean water level
        /// </remarks>
        public float UnderWaterPercentage { get; set; } = 0.40f;

        /// <summary>
        /// The amount of terrain that is supposed to be under the tree line
        /// </summary>
        /// <remarks>
        /// The tree line is the border between normal land and mountains
        /// </remarks>
        public float TreeLinePercentage { get; set; } = 0.90f;
    }
}