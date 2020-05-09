using System;
using Engine.Core;
using Game.Core;
using Game.Simulation;

namespace Game.WorldGen
{
    /// <summary>
    /// Delegate used for event that is fired when map generation is finished
    /// </summary>
    /// <param name="world">World that was generated</param>
    public delegate void WorldGenerationFinishedHandler(World world);

    /// <summary>
    /// Delegate used for event that is fired every time the map generator advances to
    /// the next generation stage
    /// </summary>
    /// <param name="statusText">Short text describing the new stage</param>
    /// <param name="progress">Overall map generation progress, in [0, 1]</param>
    public delegate void WorldGenerationStageHandler(string statusText, double progress);
    
    /// <summary>
    /// Class implementing an async operation generation the game world
    /// </summary>
    public partial class WorldGenerator : AsyncOperation
    {
        /// <summary>
        /// Is fired when the map generation is finished.
        /// </summary>
        public event WorldGenerationFinishedHandler WorldGenerationFinished;

        /// <summary>
        /// Is fired when the map generator advances to the next generation stage.
        /// </summary>
        public event WorldGenerationStageHandler WorldGenerationStageChanged;
        
        /// <summary>
        /// The desired world size, in tiles
        /// </summary>
        protected Size WorldDimensions { get; }
        
        /// <summary>
        /// Seed to use during world generation
        /// </summary>
        protected int Seed { get; }
        
        /// <summary>
        /// Create anew world generator instance
        /// </summary>
        /// <param name="dimensions">Dimensions of the world to generate, in tiles</param>
        /// <param name="seed">Seed to use for world generation</param>
        public WorldGenerator(Size dimensions, int seed)
        {
            this.WorldDimensions = dimensions;
            this.Seed = seed;
        }

        /// <summary>
        /// Process any pending events, and invoke them.
        /// </summary>
        /// <remarks>
        /// This needs to be called every frame as part of the game loop as long as the map generation
        /// operation is running.
        /// </remarks>
        public void ProcessEvents()
        {
            this.CallbackReceiver.ProcessCallbacks();
        }

        /// <summary>
        /// Signal that the world generator operation has advanced to a new generation stage.
        /// </summary>
        /// <param name="statusText">Short text describing the new stage</param>
        /// <param name="progress">Overall map generation progress, in [0, 1]</param>
        protected void SignalNextStage(string statusText, double progress)
        {
            this.Invoke(() =>
            {
                this.WorldGenerationStageChanged?.Invoke(statusText, progress);
            });   
        }

        /// <summary>
        /// Signal that world generation has finished.
        /// </summary>
        /// <param name="world">Newly generated world</param>
        protected void SignalFinished(World world)
        {
            this.Invoke(() =>
            {
                this.WorldGenerationFinished?.Invoke(world);
            });   
        }
    }
}