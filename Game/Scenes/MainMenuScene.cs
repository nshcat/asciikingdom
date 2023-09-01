using System;
using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Engine.Rendering;
using Engine.Resources;
using Game.Core;
using Game.Scenes.Common;
using Game.Simulation.Worlds;
using Game.Ui;
using Game.Ui.Toolkit;
using OpenToolkit.Windowing.Common.Input;

namespace Game.Scenes
{
    /// <summary>
    /// Custom UI theme for the main menu
    /// </summary>
    internal class MainMenuTheme : Theme
    {
        public override Position DrawButton(RenderCommandRecorder recorder, WidgetDrawParams widgetParams)
        {
            recorder.RecordPushFrontColor(widgetParams.IsEnabled ? this.ActiveTextColor : this.InactiveTextColor);

            var drawSelection = widgetParams.IsSelected && widgetParams.IsEnabled;

            var text = drawSelection ? $"> {widgetParams.Text} <" : widgetParams.Text;
            recorder.RecordDrawString(widgetParams.Position, text, widgetParams.Centered);

            recorder.RecordPopFrontColor();

            return new Position(widgetParams.Position.X + widgetParams.Text.Length, widgetParams.Position.Y);
        }
    }

    /// <summary>
    /// Custom UI theme for the world select popup
    /// </summary>
    internal class WorldSelectTheme : Theme
    {
        public override Position DrawButton(RenderCommandRecorder recorder, WidgetDrawParams widgetParams)
        {         
            var drawSelection = widgetParams.IsSelected && widgetParams.IsEnabled;

            if(drawSelection)
            {
                recorder.RecordPushFrontColor(UiColors.Keybinding);
                recorder.RecordDrawString(widgetParams.Position, ((char)26).ToString());
                recorder.RecordPopFrontColor();
            }

            recorder.RecordPushFrontColor(widgetParams.IsEnabled ? this.ActiveTextColor : this.InactiveTextColor);
            recorder.RecordDrawString(widgetParams.Position + new Position(2, 0), widgetParams.Text, widgetParams.Centered);
            recorder.RecordPopFrontColor();

            return new Position(widgetParams.Position.X + widgetParams.Text.Length + 2, widgetParams.Position.Y);
        }
    }

    /// <summary>
    /// Main menu scene
    /// </summary>
    public class MainMenuScene : Scene
    {
        /// <summary>
        /// The title banner
        /// </summary>
        private static string[] TitleBanner { get; } = 
            {
                "    _             _ _   _  ___                 _                 ",
                "   / \\   ___  ___(_|_) | |/ (_)_ __   __ _  __| | ___  _ __ ___  ",
                "  / _ \\ / __|/ __| | | | ' /| | '_ \\ / _` |/ _` |/ _ \\| '_ ` _ \\ ",
                " / ___ \\\\__ \\ (__| | | | . \\| | | | | (_| | (_| | (_) | | | | | |",
                "/_/   \\_\\___/\\___|_|_| |_|\\_\\_|_| |_|\\__, |\\__,_|\\___/|_| |_| |_|",
                "                                     |___/                       "
            };
        
        /// <summary>
        /// Main menu input actions
        /// </summary>
        private enum MainMenuAction
        {
            None = 0,
            MenuUp,
            MenuDown,
            MenuSelect,
            Exit
        }
        
        /// <summary>
        /// Surface for the game title banner, uses a big font
        /// </summary>
        private Surface _titleSurface;
        
        /// <summary>
        /// Surface for main menu entries, uses a small font
        /// </summary>
        private Surface _mainSurface;
        
        /// <summary>
        /// input action mapper
        /// </summary>
        private InputActionMapper<MainMenuAction> _actionMapper;

        /// <summary>
        /// Whether there currently are worlds available to load
        /// </summary>
        private bool _hasWorlds;

        /// <summary>
        /// Index of currently selected menu item
        /// </summary>
        private int _menuSelection = 0;

        /// <summary>
        /// Index of currently selected world
        /// </summary>
        private int _worldSelection = 0;

        /// <summary>
        /// Whether the world select window is currently being shown
        /// </summary>
        private bool _selectWorld = false;

        /// <summary>
        /// UI state for the main menu
        /// </summary>
        private UIState _mainMenuUI;

        /// <summary>
        /// UI state for the world selection UI
        /// </summary>
        private UIState _worldSelectUI;

        /// <summary>
        /// The custom theme for the main menu
        /// </summary>
        private MainMenuTheme _mainMenuTheme
            = new MainMenuTheme();

        /// <summary>
        /// The custom theme used for the world select popup
        /// </summary>
        private WorldSelectTheme _worldSelectTheme
            = new WorldSelectTheme();

        /// <summary>
        /// Create new main menu scene
        /// </summary>
        public MainMenuScene(Scene parent)
            : base(parent)
        {
            this.Initialize();
        }

        /// <summary>
        /// Create new main menu scene
        /// </summary>
        public MainMenuScene(SceneStack sceneStack, InputManager inputManager, ResourceManager resourceManager)
            : base(sceneStack, inputManager, resourceManager)
        {
            this.Initialize();
        }

        /// <summary>
        /// Initialize scene
        /// </summary>
        private void Initialize()
        {         
            this.InitializeUI();
        }

        /// <summary>
        /// Initialize UI states
        /// </summary>
        private void InitializeUI()
        {
            this._mainMenuUI = new UIState(this.Input);
        }

        /// <summary>
        /// Handle world selection by user
        /// </summary>
        private void HandleWorldSelection(int worldIndex)
        {
            // Get world index
            var index = WorldManager.Instance.Worlds[worldIndex].Item1;
            
            // Load world
            var state = WorldManager.Instance.LoadWorld(index);
            
            // Switch to game scene
            var gameScene = new GameScene(this, state);
            this.SceneStack.NextOperation = new SceneStackOperation.PushScene(gameScene);
            
            // Reset main menu state
            this._selectWorld = false;
        }
        
        /// <summary>
        /// Draw the title banner
        /// </summary>
        private void DrawTitle()
        {
            var dimensions = this._titleSurface.Dimensions;
            var position = new Position(dimensions.Width/2, dimensions.Height * 1/6);

            foreach (var line in TitleBanner)
            {
                this._titleSurface.DrawStringCentered(position, line, UiColors.ActiveText, DefaultColors.Black);
                position = position + new Position(0, 1);
            }
        }

        /// <summary>
        /// Create UI for the world select window
        /// </summary>
        protected void DoWorldSelectGui()
        {
            this._worldSelectUI.Begin(this._titleSurface);
            this._worldSelectUI.BeginWindow(new SizeF(0.35f, 0.35f), title: "Select World", padding: new Padding(1, 1, 1, 1), withScrollbar: true);
            this._worldSelectUI.PushTheme(this._worldSelectTheme);

            if(this._worldSelectUI.HasKey(Key.Escape))
            {
                this._selectWorld = false;
                return;
            }

            for(int worldIndex = 0; worldIndex < WorldManager.Instance.Worlds.Count; ++worldIndex)
            {
                var world = WorldManager.Instance.Worlds[worldIndex];
                var label = string.IsNullOrEmpty(world.Item2)
                    ? $"World {world.Item1}"
                    : $"World {world.Item1}: {world.Item2}";

                if(this._worldSelectUI.Button(label))
                {
                    this.HandleWorldSelection(worldIndex);
                    this._selectWorld = false;
                }
            }

            this._worldSelectUI.PopTheme();
            this._worldSelectUI.EndWindow();
            this._worldSelectUI.End();
        }

        /// <summary>
        /// Create UI for the main menu
        /// </summary>
        protected void DoMainMenuGui()
        {
            var _dimensions = this._titleSurface.Dimensions;
            var _menuBounds = new Rectangle(
                new Position(0, (int)(_dimensions.Height * (4.0f / 6.0f))),
                new Position(_dimensions.Width - 1, _dimensions.Height - 1)
            );

            this._mainMenuUI.Begin(this._titleSurface, !this._selectWorld);
            this._mainMenuUI.PushTheme(this._mainMenuTheme);
            this._mainMenuUI.BeginWindow(_menuBounds, drawBorder: false);
     
            this._mainMenuUI.HorizontalCenter();
            if (this._mainMenuUI.Button("Create new world", centered: true))
            {
                this.SceneStack.NextOperation = new SceneStackOperation.PushScene(
                    new WorldGenScene(this.SceneStack, this.Input, this.Resources)
                );
            }
            this._mainMenuUI.NextLine();

            this._mainMenuUI.HorizontalCenter();
            if (this._mainMenuUI.Button("Load World", centered: true, enabled: this._hasWorlds))
            {
                this._selectWorld = true;
                this._worldSelectUI = new UIState(this.Input);
                this._worldSelection = 0;
            }
            this._mainMenuUI.NextLine();

            this._mainMenuUI.HorizontalCenter();
            if (this._mainMenuUI.Button("Quit", centered: true))
            {
                this.Exit();
            }

            this._mainMenuUI.EndWindow();
            this._mainMenuUI.PopTheme();
            this._mainMenuUI.End();
        }

        /// <summary>
        /// Quit the game
        /// </summary>
        private void Exit()
        {
            this.SceneStack.NextOperation = new SceneStackOperation.PopScene();
        }
        
        /// <summary>
        /// Render main menu scene to screen
        /// </summary>
        public override void Render(RenderParams rp)
        {
            this._titleSurface.Clear();
            this._mainSurface.Clear();

            this.DrawTitle();

            this._mainMenuUI.Draw(this._titleSurface);

            if (this._selectWorld)
                this._worldSelectUI.Draw(this._titleSurface);
            
            this._titleSurface.Render(rp);
            this._mainSurface.Render(rp);
        }

        /// <summary>
        /// Handle logic and input update
        /// </summary>
        public override void Update(double deltaTime)
        {
            if (this._selectWorld)
                this.DoWorldSelectGui();

            this.DoMainMenuGui();   
        }

        /// <summary>
        /// Handle screen resize
        /// </summary>
        public override void Reshape(Size newSize)
        {
            base.Reshape(newSize);

            this._mainSurface?.Destroy();
            this._titleSurface?.Destroy();

            this._titleSurface = Surface.New()
                .Tileset(this.Resources, "myne.png")
                .PixelDimensions(this.ScreenDimensions)
                .Build();
            
            this._mainSurface = Surface.New()
                .Tileset(this.Resources, "default.png")
                .PixelDimensions(this.ScreenDimensions)
                .Transparent()
                .Build();
        }

        /// <summary>
        /// Called every time this scene gains focus
        /// </summary>
        public override void Activate()
        {
            // Refresh world managers state to detect freshly generated worlds
            WorldManager.Instance.RefreshWorlds();
            this._hasWorlds = WorldManager.Instance.HasWorlds;
        }
    }
}