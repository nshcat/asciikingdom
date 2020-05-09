using Game.Simulation;

namespace Game.WorldGen
{
    public partial class WorldGenerator
    {
        /// <summary>
        /// The actual world generation process
        /// </summary>
        protected override void DoOperation()
        {
            // Generate empty world object
            var world = new World(this.WorldDimensions, this.Seed);
            
            // Generate terrain
            this.SignalNextStage("Generating terrain..", 0.0);
            
            // Signal that world generation has finished
            this.SignalFinished(world);
        }
    }
}