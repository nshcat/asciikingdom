using Engine.Core;
using Engine.Rendering;
using OpenToolkit;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Graphics;
using OpenToolkit.Input;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Desktop;

namespace Engine
{
    /// <summary>
    /// Main class for implementing ASCII games using this project. Concrete games should derive from this
    /// class.
    /// </summary>
    public class AsciiGame : GameWindow
    {


        #region Rendering Properties
        private OrthographicProjection Projection { get; set; } = new OrthographicProjection();
        

        #endregion
        
        
        /// <summary>
        /// Create a new ascii game instance. This will create and show a window.
        /// </summary>
        /// <param name="windowDimensions">Initial window dimensions</param>
        /// <param name="title">Window title</param>
        public AsciiGame(Size windowDimensions, string title)
            : base(
                GameWindowSettings.Default,
                new NativeWindowSettings
                {
                    Location = new Vector2i(windowDimensions.Width, windowDimensions.Height),
                    Title =  title
                }
            )
        {
            
        }
        
        #region Event Callbacks

        /// <summary>
        /// Perform game logic based on given elapsed delta time.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last frame, in seconds</param>
        protected virtual void OnLogic(float deltaTime)
        {
            return;
        }

        /// <summary>
        /// Render game scene to screen. The game has to call render method of its active surfaces.
        /// </summary>
        protected virtual void OnRender()
        {
            return;
        }

        /// <summary>
        /// React to game window resolution change.
        /// </summary>
        /// <param name="newDimensions">New window dimensions, in pixels</param>
        protected virtual void OnReshape(Size newDimensions)
        {
            return;
        }
        
        #endregion
    }
}