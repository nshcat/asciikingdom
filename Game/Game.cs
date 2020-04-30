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
            
        }

        /// <summary>
        /// React to window size change
        /// </summary>
        /// <param name="newDimensions"></param>
        protected override void OnReshape(Size newDimensions)
        {
            this.MainSurface?.Destroy();
            
            this.MainSurface = Surface.New()
                .Tileset(this.Resources, "default.png")
                .PixelDimensions(this.WindowDimensions)
                .Build();
        }

        /// <summary>
        /// Render frame
        /// </summary>
        /// <param name="renderParams">Rendering parameters to use</param>
        protected override void OnRender(RenderParams renderParams)
        {
            this.MainSurface.Clear();
            
            this.MainSurface.DrawString(new Position(2, 2), this._lastAction, DefaultColors.White, DefaultColors.Black);

            var tile = new Tile('X', DefaultColors.White, DefaultColors.Black);
            
            for (int ix = 0; ix < this.MainSurface.Dimensions.Width; ++ix)
            {
                for (int iy = 0; iy < this.MainSurface.Dimensions.Height; ++iy)
                {
                    if ((iy == 0 || iy == this.MainSurface.Dimensions.Height - 1) ||
                        (ix == 0 || ix == this.MainSurface.Dimensions.Width - 1))
                    {
                        this.MainSurface.SetTile(new Position(ix, iy), tile);
                    }
                }
            }
            
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