using System;
using Engine;
using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Engine.Rendering;
using Game.Core;
using Game.Data;
using Game.Scenes;
using Game.Settings;
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
            : base(
                new Size(
                    SettingsManager.Instance.Settings.General.GameWindowWidth,
                    SettingsManager.Instance.Settings.General.GameWindowHeight
                ),
                "AsciiKingdom")
        {
        }

        /// <summary>
        /// Initial game setup
        /// </summary>
        protected override void OnSetup()
        {
            // Load data files
            this.LoadData();
            
            var initialScene = new MainMenuScene(this.SceneStack, this.Input, this.Resources);
            this.SceneStack.AddInitialScene(initialScene);
        }

        /// <summary>
        /// Load game data from filesystem
        /// </summary>
        protected void LoadData()
        { 
            ResourceTypeManager.Instance.LoadTypes(this.Resources);
            ProductTypeManager.Instance.LoadTypes(this.Resources);
            CropTypeManager.Instance.LoadTypes(this.Resources);
            SiteTypeManager.Instance.LoadTypes(this.Resources);
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
            
            if(this.SceneStack.IsEmpty)
                this.Close();
        }
    }
}