namespace Game.WorldGen
{
    public partial class WorldGenerator
    {
        /// <summary>
        /// Represents the different temperature levels that can appear
        /// on the world map
        /// </summary>
        private enum TemperatureLevel
        {
            Coldest,
            Colder,
            Cold,
            Warm,
            Warmer,
            Warmest
        }

        /// <summary>
        /// Represents the different moisture levels that can appear on
        /// the world map
        /// </summary>
        private enum MoistureLevel
        {
            Wettest,
            Wetter,
            Wet,
            Dry,
            Dryer,
            Dryest
        }
    }
}