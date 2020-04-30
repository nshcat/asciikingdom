using System;
using Engine;
using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Engine.Rendering;
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
        /// The main game surface
        /// </summary>
        protected Surface MainSurface { get; set; }

        protected enum MainAction
        {
            None = 0,
            MoveUp,
            MoveDown,
            MoveRight,
            MoveLeft,
            MoveUpFast,
            MoveDownFast,
            MoveRightFast,
            MoveLeftFast
        }

        private InputActionMapper<MainAction> _actionMapper;
        private string _lastAction = "";
        
        /// <summary>
        /// Construct new game instance
        /// </summary>
        public Game()
            : base(new Size(1024, 640), "AsciiKingdom")
        {
            this._actionMapper = new InputActionMapper<MainAction>(this.Input,
                new InputAction<MainAction>(MainAction.MoveUp, KeyPressType.Pressed, Key.Up),
                new InputAction<MainAction>(MainAction.MoveDown, KeyPressType.Pressed, Key.Down),
                new InputAction<MainAction>(MainAction.MoveLeft, KeyPressType.Pressed, Key.Left),
                new InputAction<MainAction>(MainAction.MoveRight, KeyPressType.Pressed, Key.Right),
                new InputAction<MainAction>(MainAction.MoveUpFast, KeyPressType.Pressed, Key.Up, Key.ShiftLeft),
                new InputAction<MainAction>(MainAction.MoveDownFast, KeyPressType.Pressed, Key.Down, Key.ShiftLeft),
                new InputAction<MainAction>(MainAction.MoveLeftFast, KeyPressType.Pressed, Key.Left, Key.ShiftLeft),
                new InputAction<MainAction>(MainAction.MoveRightFast, KeyPressType.Pressed, Key.Right, Key.ShiftLeft)
            );
        }

        /// <summary>
        /// Initial game setup
        /// </summary>
        protected override void OnSetup()
        {
            // Create main surface
            this.MainSurface = Surface.New()
                .Tileset(this.Resources, "default.png")
                .TileDimensions(25, 10)
                .Build();
        }

        /// <summary>
        /// Render frame
        /// </summary>
        /// <param name="renderParams">Rendering parameters to use</param>
        protected override void OnRender(RenderParams renderParams)
        {
            this.MainSurface.Clear();
            
            this.MainSurface.DrawString(Position.Origin, this._lastAction, DefaultColors.White, DefaultColors.Black);
            
            this.MainSurface.Render(renderParams);
        }

        /// <summary>
        /// Update game logic
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last frame, in seconds</param>
        protected override void OnLogic(double deltaTime)
        {
            this._actionMapper.Update();

            if (this._actionMapper.HasTriggeredAction)
            {
                _lastAction = this._actionMapper.TriggeredAction.ToString();
                Console.WriteLine($"New Action: {_lastAction}");
            }
        }
    }
}