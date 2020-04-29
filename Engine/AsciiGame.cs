using System;
using Engine.Core;
using Engine.Graphics;
using Engine.Rendering;
using Engine.Resources;
using OpenToolkit;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Graphics;
using OpenToolkit.Input;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;

namespace Engine
{
    /// <summary>
    /// Main class for implementing ASCII games using this project. Concrete games should derive from this
    /// class.
    /// </summary>
    public class AsciiGame : GameWindow
    {
        /// <summary>
        /// The title of the game
        /// </summary>
        public string GameTitle { get; protected set; }
        
        /// <summary>
        /// The resource manager
        /// </summary>
        public ResourceManager Resources { get; protected set; } = new ResourceManager();

        /// <summary>
        /// The orthographic projection used to render ASCII surfaces
        /// </summary>
        private OrthographicProjection Projection { get; set; } = new OrthographicProjection();
        
        
        /// <summary>
        /// Create a new ascii game instance. This will create and show a window.
        /// </summary>
        /// <param name="windowDimensions">Initial window dimensions</param>
        /// <param name="title">Window title</param>
        public AsciiGame(Size windowDimensions, string gameTitle)
            : base(
                GameWindowSettings.Default,
                new NativeWindowSettings
                {
                    Location = new Vector2i(windowDimensions.Width, windowDimensions.Height),
                    Title =  gameTitle
                }
            )
        {
            this.GameTitle = gameTitle;
        }


        /// <summary>
        /// Build window title string containing the current FPS and OpenGL version
        /// </summary>
        /// <param name="frameTime">Time elapsed since last frame</param>
        /// <returns>New window title string</returns>
        protected string BuildTitle(double frameTime)
        {
            var fps = 1.0 / frameTime;
            return $"{this.GameTitle} - {fps:0} FPS - OpenGL {GL.GetString(StringName.Version)}";
        }

        #region OpenTK Window Events

        /// <summary>
        /// Handle logic update request
        /// </summary>
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            this.OnLogic(args.Time);
        }

        /// <summary>
        /// Handle render frame request
        /// </summary>
        /// <param name="args"></param>
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            this.Title = this.BuildTitle(args.Time);

            var backColor = DefaultColors.Black.ToVector4F();
            GL.ClearColor((Color4)backColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            base.OnRenderFrame(args);
            this.OnRender(this.Projection.Params);
            
            GL.Flush();
            
            SwapBuffers();
        }

        /// <summary>
        /// Handle window initialization
        /// </summary>
        protected override void OnLoad()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            
            this.OnSetup();
        }

        /// <summary>
        /// Handle window resize
        /// </summary>
        protected override void OnResize(ResizeEventArgs e)
        {
            var newSize = new Size(this.Size.X, this.Size.Y);
            GL.Viewport(0, 0, newSize.Width, newSize.Height);
            this.Projection.Refresh(newSize);
            this.OnReshape(newSize);
            base.OnResize(e);
        }

        #endregion
        
        
        #region Event Callbacks

        /// <summary>
        /// Perform initial game setup. This is called after OpenGL has been initialized.
        /// </summary>
        protected virtual void OnSetup()
        {
            return;
        }

        /// <summary>
        /// Perform game logic based on given elapsed delta time.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last frame, in seconds</param>
        protected virtual void OnLogic(double deltaTime)
        {
            return;
        }

        /// <summary>
        /// Render game scene to screen. The game has to call render method of its active surfaces.
        /// </summary>
        /// <param name="renderParams">Rendering parameters to use for rendering</param>
        protected virtual void OnRender(RenderParams renderParams)
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