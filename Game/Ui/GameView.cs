using Engine.Core;
using Engine.Graphics;
using Game.Core;

namespace Game.Ui
{
    /// <summary>
    /// Directions the view cursor can be moved in
    /// </summary>
    public enum MovementDirection
    {
        Up,
        Down,
        Left,
        Right
    }
    
    /// <summary>
    /// Cursor display modes
    /// </summary>
    public enum CursorMode
    {
        /// <summary>
        /// Cursor is not shown
        /// </summary>
        Disabled,
        
        /// <summary>
        /// Normal, yellow cursor
        /// </summary>
        Normal,
        
        /// <summary>
        /// Red cursor, used to indicate things like invalid site placement
        /// </summary>
        Invalid
    }

    /// <summary>
    /// Delegate used for the CursorMoved event in <see cref="GameView"/>
    /// </summary>
    /// <param name="newPosition"></param>
    public delegate void CursorMovedHandler(Position newPosition);
    
    /// <summary>
    /// Base class for all UI elements that show components of the simulation state, such as world terrain and sites.
    /// </summary>
    public abstract class GameView : SceneComponent, ILogic
    {
        /// <summary>
        /// Event that is fired every time the cursor is moved.
        /// </summary>
        public event CursorMovedHandler CursorMoved;
        
        /// <summary>
        /// The dimensions of the game world
        /// </summary>
        public Size WorldDimensions { get; protected set; }
        
        /// <summary>
        /// The dimensions of this view, halved
        /// </summary>
        protected Size HalfDimensions { get; set; }

        /// <summary>
        /// The current position of the cursor, in absolute world coordinates
        /// </summary>
        /// <remarks>
        /// When setting this value from outside this class, the method <see cref="RecalulatePositions"/>
        /// has to be called afterwards.
        /// </remarks>
        public Position CursorPosition { get; set; }
            = Position.Origin;
        
        /// <summary>
        /// The position of the cursor on the screen
        /// </summary>
        protected Position CursorScreenPosition { get; set; }
        
        /// <summary>
        /// The world position of the top left view corner
        /// </summary>
        protected Position TopLeft { get; set; }
        
        /// <summary>
        /// The current cursor display mode
        /// </summary>
        public CursorMode CursorMode { get; set; } = CursorMode.Normal;

        /// <summary>
        /// Whether this view is enabled and should be drawn
        /// </summary>
        public bool Enabled { get; set; }
            = true;
        
        /// <summary>
        /// The tile to use to draw the cursor in normal mode
        /// </summary>
        public Tile NormalCursorTile { get; set; } = new Tile(88, DefaultColors.Yellow, DefaultColors.Black);
        
        /// <summary>
        /// The tile to use to draw the cursor in invalid mode
        /// </summary>
        public Tile InvalidCursorTile { get; set; } = new Tile(88, DefaultColors.Red, DefaultColors.Black);

        /// <summary>
        /// The tile to use to draw the cursor. Depends on the value of see <see cref="CursorMode"/>.
        /// </summary>
        protected Tile CursorTile => (this.CursorMode == CursorMode.Normal) ? this.NormalCursorTile : InvalidCursorTile;

        /// <summary>
        /// Timer used to blink cursor
        /// </summary>
        protected ToggleTimer CursorTimer { get; set; } = new ToggleTimer(0.25, true);
        
        /// <summary>
        /// Base class constructor
        /// </summary>
        public GameView(Position position, Size dimensions, Size worldDimensions)
            : base(position, dimensions)
        {
            this.WorldDimensions = worldDimensions;
            this.HalfDimensions = new Position(this.Dimensions.Width / 2, this.Dimensions.Height / 2);
            this.Recenter();
        }
        
        /// <summary>
        /// Centers the view to the center of the world
        /// </summary>
        public void Recenter()
        {
            this.CursorPosition = new Rectangle(this.WorldDimensions).Center;
            this.RecalulatePositions();
        }

        /// <summary>
        /// Force firing of the cursor moved event
        /// </summary>
        public void FireCursorMovedEvent()
        {
            this.CursorMoved?.Invoke(this.CursorPosition);
        }

        /// <summary>
        /// Recalculate various positions based on the current value of <see cref="CursorPosition"/>
        /// </summary>
        /// <remarks>
        /// This needs to be called every time the cursor position, view dimensions or position are changed from
        /// outside this class.
        /// </remarks>
        public void RecalulatePositions()
        {
            this.HalfDimensions = new Position(this.Dimensions.Width / 2, this.Dimensions.Height / 2);
            
            var max = this.WorldDimensions - this.Dimensions;

            this.TopLeft = (this.CursorPosition - (Position) this.HalfDimensions)
                .Clamp(Position.Origin, max);

            this.CursorScreenPosition = (CursorPosition - this.TopLeft) + this.Position;
            
            this.CursorMoved?.Invoke(this.CursorPosition);
        }

        /// <summary>
        /// Move the view cursor in given direction and distance.
        /// </summary>
        public void MoveCursor(MovementDirection direction, int distance = 1)
        {
            var vector = direction switch
            {
                MovementDirection.Down => new Position(0, 1),
                MovementDirection.Up => new Position(0, -1),
                MovementDirection.Left => new Position(-1, 0),
                MovementDirection.Right => new Position(1, 0)
            };

            this.CursorPosition = (this.CursorPosition + (vector * distance))
                .Clamp(Position.Origin, (Position)this.WorldDimensions - new Position(1, 1));
            
            this.RecalulatePositions();
        }

        /// <summary>
        /// Draw the cursor in the specified style
        /// </summary>
        protected void DrawCursor(Surface surface)
        {
            if (this.CursorMode != CursorMode.Disabled && this.CursorTimer.Flag)
            {
                surface.SetTile(this.CursorScreenPosition, this.CursorTile);
            }
        }

        /// <summary>
        /// Render game view to given surface
        /// </summary>
        /// <param name="surface">Surface to render to</param>
        public override void Render(Surface surface)
        {
            if (!this.Enabled || !this.ShouldRender())
                return;
            
            this.BeforeRender(surface);

            var worldBounds = new Rectangle(this.WorldDimensions);

            for (var iy = 0; iy < this.Dimensions.Height; ++iy)
            {
                for (var ix = 0; ix < this.Dimensions.Width; ++ix)
                {
                    var localPosition = new Position(ix, iy);
                    var screenPosition = localPosition + this.Position;
                    var worldPosition = localPosition + this.TopLeft;
                    
                    if(worldPosition.IsInBounds(worldBounds))
                        this.RenderCell(surface, worldPosition, screenPosition);
                }
            }

            // Perform any after render steps
            this.AfterRender(surface);
            
            // Finally, draw the cursor, if enabled
            this.DrawCursor(surface);
        }

        /// <summary>
        /// Update logic
        /// </summary>
        public virtual void Update(double deltaTime)
        {
            this.CursorTimer.Update(deltaTime);
        }

        /// <summary>
        /// Perform view rendering for given world cell
        /// </summary>
        /// <param name="surface">The surface to draw to</param>
        /// <param name="worldPosition">The cells world position</param>
        /// <param name="screenPosition">The corresponding position on the screen</param>
        protected abstract void RenderCell(Surface surface, Position worldPosition, Position screenPosition);

        /// <summary>
        /// Perform any rendering actions after the main view has been drawn
        /// </summary>
        /// <param name="surface">The surface to draw to</param>
        protected virtual void AfterRender(Surface surface)
        {
            
        }
        
        /// <summary>
        /// Perform any rendering actions before the main view will be drawn
        /// </summary>
        /// <param name="surface">The surface to draw to</param>
        protected virtual void BeforeRender(Surface surface)
        {
            
        }

        /// <summary>
        /// Whether the view should render itself right now.
        /// </summary>
        /// <remarks>
        /// This is used for views that sometimes do not have any data attached to them.
        /// </remarks>
        protected abstract bool ShouldRender();
    }
}