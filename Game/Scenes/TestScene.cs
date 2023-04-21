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
            var log = LogManager.GetCurrentClassLogger();

            /*this._ui.Begin();
            this._ui.Label("Label 1");
            if (this._ui.Button("Button 1"))
                log.Info("Button 1 pressed");
            this._ui.Checkbox("Checkbox 1", ref this._flag);
            this._ui.PushIndent();
                if(this._ui.Button("Button 2"))
                    log.Info("Button 2 pressed");
                this._ui.Label("Label 2");
                this._ui.PushIndent();
                    if(this._ui.Button("Button 3"))
                        log.Info("Button 3 pressed");
                this._ui.PopIndent();
                if(this._ui.Button("Button 4"))
                    log.Info("Button 4 pressed");
            this._ui.PopIndent();
            this._ui.Label("Label 3");
            this._ui.End();*/

            var bounds = this._mainSurface.Bounds.Centered(
                this._mainSurface.Dimensions * 0.35f
            );

            this._ui.Begin(this._mainSurface);
            this._ui.Window("Test", new SizeF(0.35f, 0.35f), new Padding(1, 1, 1, 1));
            this._ui.Label("Label");
            this._ui.Button("Button 1");
            this._ui.Button("Button 2");
            this._ui.Button("Button 3");
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
                    SettingsManager.Instance.Settings.General.TextTileset)
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