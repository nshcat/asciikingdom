using System;
using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Engine.Rendering;
using Engine.Resources;
using Game.Core;
using Game.Simulation;
using Game.Ui;
using OpenToolkit.Windowing.Common.Input;
using SixLabors.ImageSharp.Processing.Processors.Dithering;

namespace Game.Scenes
{
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
            this.InitializeMapper();
        }

        /// <summary>
        /// Initialize the input mapper
        /// </summary>
        private void InitializeMapper()
        {
            this._actionMapper = new InputActionMapper<MainMenuAction>(this.Input,
                new InputAction<MainMenuAction>(MainMenuAction.MenuUp, KeyPressType.Down, Key.Up),
                new InputAction<MainMenuAction>(MainMenuAction.MenuDown, KeyPressType.Down, Key.Down),
                new InputAction<MainMenuAction>(MainMenuAction.MenuSelect, KeyPressType.Pressed, Key.Enter),
                new InputAction<MainMenuAction>(MainMenuAction.Exit, KeyPressType.Pressed, Key.Escape),
                new InputAction<MainMenuAction>(MainMenuAction.Exit, KeyPressType.Down, Key.Q, Key.ShiftLeft)
            );
        }

        /// <summary>
        /// Calculate the next selection index
        /// </summary>
        private void NextMenuSelectionIndex(int direction)
        {
            var nextIndex = (this._menuSelection + direction);
            
            if (nextIndex > 2)
                nextIndex = 0;
            if (nextIndex < 0)
                nextIndex = 2;

            // Jump over disabled "load world" if no worlds are available
            if (nextIndex == 1 && !this._hasWorlds)
                nextIndex += direction;

            this._menuSelection = nextIndex;
        }
        
        /// <summary>
        /// Calculate the next world index
        /// </summary>
        private void NextWorldSelectionIndex(int direction)
        {
            var nextIndex = (this._worldSelection + direction);

            if (nextIndex > WorldManager.Instance.Worlds.Count - 1)
                nextIndex = 0;
            if (nextIndex < 0)
                nextIndex = WorldManager.Instance.Worlds.Count - 1;
            
            this._worldSelection = nextIndex;
        }

        /// <summary>
        /// Handle user input action
        /// </summary>
        private void HandleInput(MainMenuAction action)
        {
            switch (action)
            {
                case MainMenuAction.MenuUp:
                    if(this._selectWorld)
                        this.NextWorldSelectionIndex(-1);
                    else
                        this.NextMenuSelectionIndex(-1);
                    break;
                case MainMenuAction.MenuDown:
                    if(this._selectWorld)
                        this.NextWorldSelectionIndex(1);
                    else
                        this.NextMenuSelectionIndex(1);
                    break;
                case MainMenuAction.MenuSelect:
                    if (this._selectWorld)
                        this.HandleWorldSelection();
                    else
                        this.HandleMenuSelection();
                    break;
                case MainMenuAction.Exit:
                    if (this._selectWorld)
                        this._selectWorld = false;
                    else
                        this.Exit();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handle world selection by user
        /// </summary>
        private void HandleWorldSelection()
        {
            // Get world index
            var index = WorldManager.Instance.Worlds[this._worldSelection].Item1;
            
            // Load world
            var state = WorldManager.Instance.LoadWorld(index);
            
            // Switch to game scene
            var gameScene = new GameScene(this, state);
            this.SceneStack.NextOperation = new SceneStackOperation.PushScene(gameScene);
            
            // Reset main menu state
            this._selectWorld = false;
            this._menuSelection = 0;
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
        /// Draw a menu button
        /// </summary>
        private void DrawButton(Position basePosition, int index, bool enabled, string text)
        {
            var position = basePosition + new Position(0, index * 2);
            var frontColor = enabled ? UiColors.ActiveText : UiColors.InactiveText;
            var buttonText = (index == this._menuSelection) ? $"> {text} <" : text;
            this._titleSurface.DrawStringCentered(position, buttonText, frontColor, DefaultColors.Black);
        }
        
        /// <summary>
        /// Draw the menu
        /// </summary>
        private void DrawMenu()
        {
            var dimensions = this._titleSurface.Dimensions;
            var position = new Position(dimensions.Width/2, dimensions.Height * 4/6);
            
            this.DrawButton(position, 0, true, "Create new world");
            this.DrawButton(position, 1, this._hasWorlds, "Load world");
            this.DrawButton(position, 2, true, "Quit");
        }

        /// <summary>
        /// Draw the world selection screen
        /// </summary>
        private void DrawWorldSelection()
        {
            // TODO: Scroll bar
            
            var bounds = this._titleSurface.Bounds.Centered(
                this._titleSurface.Dimensions * 0.35f
            );
            
            this._titleSurface.DrawWindow(bounds, "Select World", UiColors.BorderFront, UiColors.BorderBack, UiColors.BorderTitle, DefaultColors.Black);

            var basePosition = bounds.TopLeft + new Position(4, 2);

            for (var iy = 0; iy < WorldManager.Instance.Worlds.Count; ++iy)
            {
                var position = basePosition + new Position(0, iy);
                var world = WorldManager.Instance.Worlds[iy];

                var frontColor = UiColors.InactiveText;
                
                if (iy == this._worldSelection)
                {
                    frontColor = UiColors.ActiveText;
                    var arrowPosition = position + new Position(-2, 0);
                    this._titleSurface.DrawString(arrowPosition, ((char)26).ToString(), UiColors.Keybinding, DefaultColors.Black);
                }
                
                if(!string.IsNullOrEmpty(world.Item2))
                    this._titleSurface.DrawString(position, $"World {world.Item1}: {world.Item2}", frontColor, DefaultColors.Black);
                else
                    this._titleSurface.DrawString(position, $"World {world.Item1}", frontColor, DefaultColors.Black);
            }
        }

        /// <summary>
        /// Handle menu entry selection by the user
        /// </summary>
        private void HandleMenuSelection()
        {
            switch (this._menuSelection)
            {
                case 0:
                    this.SceneStack.NextOperation = new SceneStackOperation.PushScene(
                        new WorldGenScene(this.SceneStack, this.Input, this.Resources)
                    );
                    break;
                case 1:
                    this._selectWorld = true;
                    this._worldSelection = 0;
                    break;
                case 2:
                    this.Exit();
                    break;
                default:
                    break;
            }
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
            this.DrawMenu();
            
            if(this._selectWorld)
                this.DrawWorldSelection();
            
            this._titleSurface.Render(rp);
            this._mainSurface.Render(rp);
        }

        /// <summary>
        /// Handle logic and input update
        /// </summary>
        public override void Update(double deltaTime)
        {
            if(this.Input.AreKeysDown(KeyPressType.Pressed, Key.T))
                this.SceneStack.NextOperation = new SceneStackOperation.PushScene(
                    new TestTabContainer(this)
                    );
            
            this._actionMapper.Update();
            
            if (this._actionMapper.HasTriggeredAction)
            {
                this.HandleInput(this._actionMapper.TriggeredAction);
            }
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