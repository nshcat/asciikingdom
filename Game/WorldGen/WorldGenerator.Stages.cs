using System.Threading;
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
            
            Thread.Sleep(1000);
            this.SignalNextStage("Nya..", 0.05);
            Thread.Sleep(1000);
            this.SignalNextStage("Woof..", 0.15);
            Thread.Sleep(1000);
            this.SignalNextStage("Meow..", 0.3);
            Thread.Sleep(1000);
            this.SignalNextStage("Mrowr..", 0.3);
            Thread.Sleep(1000);
            this.SignalNextStage("Nya..", 0.5);
            Thread.Sleep(1000);
            this.SignalNextStage("Woof..", 0.7);
            Thread.Sleep(1000);
            this.SignalNextStage("Purr..", 0.85);
            Thread.Sleep(1000);
            this.SignalNextStage("Bark..", 0.9);
            Thread.Sleep(1000);
            this.SignalNextStage("Bark..", 1.0);
            Thread.Sleep(1000);
            
            // Signal that world generation has finished
            this.SignalFinished(world);
        }
    }
}