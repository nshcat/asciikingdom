using System;
using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Engine.Rendering;
using Engine.Resources;
using Game.Core;
using Game.Scenes.Common;
using Game.Settings;
using Game.Simulation;
using Game.Ui;
using Game.Ui.Toolkit;
using NLog;
using OpenToolkit.Windowing.Common.Input;
using SixLabors.ImageSharp.Processing.Processors.Dithering;

namespace Game.Scenes
{
    /// <summary>
    /// Main menu scene
    /// </summary>
    public class TestScene : Scene
    {  
        /// <summary>
        /// Surface for main menu entries, uses a small font
        /// </summary>
        private Surface _mainSurface;

        private UIState _ui;
          
        /// <summary>
        /// Create new main menu scene
        /// </summary>
        public TestScene(Scene parent)
            : base(parent)
        {
            this.Initialize();
        }

        /// <summary>
        /// Create new main menu scene
        /// </summary>
        public TestScene(SceneStack sceneStack, InputManager inputManager, ResourceManager resourceManager)
            : base(sceneStack, inputManager, resourceManager)
        {
            this.Initialize();
        }

        /// <summary>
        /// Initialize scene
        /// </summary>
        private void Initialize()
        {
            this._ui = new UIState(this.Input);
        }

        bool _flag = false;

        /// <summary>
        /// Handle logic and input update
        /// </summary>
        public override void Update(double deltaTime)
        {
            this._ui.Begin(this._mainSurface);
            this._ui.BeginWindow(new SizeF(0.35f, 0.35f), title: "Test", new Padding(1, 1, 1, 1), withScrollbar: true);
            this._ui.Label("Label");
            for (int i = 0; i < 17; ++i)
            {
                this._ui.HorizontalCenter(); this._ui.Button($"Button {i + 1}", centered: true);
            }
            this._ui.EndWindow();
            this._ui.End();
        }

        /// <summary>
        /// Render main menu scene to screen
        /// </summary>
        public override void Render(RenderParams rp)
        {
            this._mainSurface.Clear();

            this._ui.Draw(this._mainSurface);

            this._mainSurface.Render(rp);
        }

        /// <summary>
        /// Handle screen resize
        /// </summary>
        public override void Reshape(Size newSize)
        {
            base.Reshape(newSize);

            this._mainSurface?.Destroy();

            this._mainSurface = Surface.New()
                .Tileset(this.Resources,
                    SettingsManager.Instance.Settings.General.GraphicsTileset)
                .PixelDimensions(this.ScreenDimensions)
                .Transparent()
                .Build();
        }

        /// <summary>
        /// Called every time this scene gains focus
        /// </summary>
        public override void Activate()
        {
        }
    }
}