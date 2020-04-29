using Engine;
using Engine.Core;
using Engine.Graphics;
using Engine.Rendering;
using OpenToolkit.Graphics.OpenGL4;

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
        
        /// <summary>
        /// Construct new game instance
        /// </summary>
        public Game()
            : base(new Size(1024, 640), "AsciiKingdom")
        {
            
        }

        /// <summary>
        /// Initial game setup
        /// </summary>
        protected override void OnSetup()
        {
            // Retrieve surface tileset
            var tileset = this.Resources.GetTileset("default.png");
            
            // Create main surface
            this.MainSurface = new Surface(Position.Origin, new Size(10, 10), tileset);
        }

        /// <summary>
        /// Render frame
        /// </summary>
        /// <param name="renderParams"></param>
        protected override void OnRender(RenderParams renderParams)
        {
            this.MainSurface.Clear();
            
            var tile = new Tile(1, DefaultColors.Red, DefaultColors.Green);
            this.MainSurface.SetTile(Position.Origin, tile);
            
            this.MainSurface.Render(renderParams);
        }
    }
}