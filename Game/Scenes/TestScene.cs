using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Engine.Rendering;
using Engine.Resources;
using Game.Core;
using OpenToolkit.Windowing.Common.Input;

namespace Game.Scenes
{
    /// <summary>
    /// Scene for testings things
    /// </summary>
    public class TestScene : Scene
    {
        /// <summary>
        /// Actions available in this scene
        /// </summary>
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

        // Action mapper for this scene
        private InputActionMapper<MainAction> _actionMapper;
        private string _lastAction = "";
        
        // The surface used to draw to the screen
        private Surface _rootSurface;
        
        /// <summary>
        /// Create new test scene
        /// </summary>
        public TestScene(Scene parent) : base(parent)
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

        public TestScene(SceneStack sceneStack, InputManager inputManager, ResourceManager resourceManager) : base(sceneStack, inputManager, resourceManager)
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

        public override void Render(RenderParams rp)
        {
            this._rootSurface.Clear();
            
            this._rootSurface.DrawString(new Position(2, 2), this._lastAction, DefaultColors.White, DefaultColors.Black);

            var tile = new Tile('X', DefaultColors.White, DefaultColors.Black);
            
            for (int ix = 0; ix < this._rootSurface.Dimensions.Width; ++ix)
            {
                for (int iy = 0; iy < this._rootSurface.Dimensions.Height; ++iy)
                {
                    if ((iy == 0 || iy == this._rootSurface.Dimensions.Height - 1) ||
                        (ix == 0 || ix == this._rootSurface.Dimensions.Width - 1))
                    {
                        this._rootSurface.SetTile(new Position(ix, iy), tile);
                    }
                }
            }
            
            this._rootSurface.Render(rp);
        }

        public override void Update(double deltaTime)
        {
            this._actionMapper.Update();

            if (this._actionMapper.HasTriggeredAction)
            {
                _lastAction = this._actionMapper.TriggeredAction.ToString();
            }
        }

        public override void Reshape(Size newSize)
        {
            base.Reshape(newSize);
            
            this._rootSurface?.Destroy();
            
            this._rootSurface = Surface.New()
                .Tileset(this.Resources, "default.png")
                .PixelDimensions(this.ScreenDimensions)
                .Build();
        }
    }
}