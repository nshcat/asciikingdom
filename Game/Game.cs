using System;
using Engine;
using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Engine.Rendering;
using Game.Core;
using Game.Scenes;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.Common.Input;

namespace Game
{
    /// <summary>
    /// Main game class
    /// </summary>
    public class Game: AsciiGame
    {
        /// <summary>
        /// Stack of game scenes making up the game.
        /// </summary>
        protected SceneStack SceneStack { get; set; } = new SceneStack();
        
        /// <summary>
        /// Construct new game instance
        /// </summary>
        public Game()
            : base(new Size(1400, 860), "AsciiKingdom")
        {
        }

        /// <summary>
        /// Initial game setup
        /// </summary>
        protected override void OnSetup()
        {
            var initialScene = new MapViewerScene(this.SceneStack, this.Input, this.Resources);
            this.SceneStack.AddInitialScene(initialScene);
        }

        /// <summary>
        /// React to window size change
        /// </summary>
        /// <param name="newDimensions"></param>
        protected override void OnReshape(Size newDimensions)
        {
            this.SceneStack.Reshape(newDimensions);
        }

        /// <summary>
        /// Render frame
        /// </summary>
        /// <param name="renderParams">Rendering parameters to use</param>
        protected override void OnRender(RenderParams renderParams)
        {
            this.SceneStack.Render(renderParams);
        }

        /// <summary>
        /// Update game logic
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last frame, in seconds</param>
        protected override void OnLogic(double deltaTime)
        {
            this.SceneStack.Update(deltaTime);
        }
    }
}