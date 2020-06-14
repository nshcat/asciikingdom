using System.Collections.Generic;
using System.Linq;
using Engine.Core;
using Engine.Graphics;
using Game.Simulation;

namespace Game.Ui
{
    /// <summary>
    /// Game view that shows the sites on the world map
    /// </summary>
    public class SiteView : GameView
    {
        /// <summary>
        /// The current simulation state
        /// </summary>
        public SimulationState State { get; set; }

        /// <summary>
        /// Whether there currently is a simulation state instance associated with this view
        /// </summary>
        public bool HasState => this.State != null;

        /// <summary>
        /// How far the cursor has to be away from a site for its map label to be drawn
        /// </summary>
        public float LabelMinDistance { get; set; } = 5.0f;
        
        /// <summary>
        /// Whether to draw city influence ranges
        /// </summary>
        public bool DrawCityInfluence { get; set; } = false;

        /// <summary>
        /// List used to remember all cities in view. This is used to draw certain overlays like influence range
        /// after the map has been drawn.
        /// </summary>
        public List<City> CitiesInView { get; set; }
            = new List<City>();
        
        /// <summary>
        /// All sites in the world
        /// </summary>
        protected Dictionary<Position, IWorldSite> Sites { get; set; }
        
        /// <summary>
        /// Create new site view based on given simulation state
        /// </summary>
        public SiteView(Position position, Size dimensions, SimulationState state)
            : base(position, dimensions, state.World.Dimensions)
        {
            
        }
        
        /// <summary>
        /// Create new site view without any simulation state associated with it
        /// </summary>
        public SiteView(Position position, Size dimensions)
            : base(position, dimensions, Size.Empty)
        {
            
        }

        /// <summary>
        /// Associated given simulation state with this site view
        /// </summary>
        public void ReplaceState(SimulationState state)
        {
            this.State = state;
            this.WorldDimensions = state.World.Dimensions;
            this.Recenter();
        }

        /// <summary>
        /// Render site map cell
        /// </summary>
        protected override void RenderCell(Surface surface, Position worldPosition, Position screenPosition)
        {
            // Check if there is a site on the current position
            if (this.Sites.ContainsKey(worldPosition))
            {
                // Retrieve site
                var site = this.Sites[worldPosition];
                
                // If its a city, save it for later to draw influence circles
                if(site is City city)
                    this.CitiesInView.Add(city);

                surface.SetTile(screenPosition, site.Tile);
                
                // Draw site name if requested and cursor is some specific distance away
                var distance = Position.GetDistance(worldPosition, this.CursorPosition);
                
                if (site.ShowName &&
                    !string.IsNullOrEmpty(site.Name) &&
                    distance >= this.LabelMinDistance)
                {
                    surface.DrawStringCentered(
                        new Position(screenPosition.X, screenPosition.Y - 2),
                        site.Name,
                        DefaultColors.Black,
                        UiColors.MapLabel
                    );

                    var half = (int) (site.Name.Length / 2);
                    for (var ix = 0; ix < site.Name.Length; ++ix)
                    {
                        var pos = new Position((screenPosition.X + ix + 1) - half, screenPosition.Y - 1);
                        surface.SetUiShadow(pos, true);
                    }
                }
            }
        }
        
        /// <summary>
        /// Symbolize influence circles, if requested
        /// </summary>
        protected void DrawInfluenceCircles(Surface surface)
        {
            if (!this.DrawCityInfluence)
                return;
            
            var circles = this.CitiesInView.Select(x => x.InfluenceCircle).ToList();
            
            for (var ix = 0; ix < this.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < this.Dimensions.Height; ++iy)
                {
                    if (!circles.Any(x => x.ContainsPoint(new Position(ix, iy) + this.TopLeft)))
                    {
                        surface.SetUiShadow(new Position(ix, iy) + this.Position, true);
                    }
                }
            }
        }
        
        /// <summary>
        /// Collect world site data before rendering
        /// </summary>
        protected override void BeforeRender(Surface surface)
        {
            this.Sites = this.State.GetAllSites();
            this.CitiesInView.Clear();
        }
        
        /// <summary>
        /// Finish rendering
        /// </summary>
        protected override void AfterRender(Surface surface)
        {
            this.DrawInfluenceCircles(surface);
        }


        /// <summary>
        /// The site view is rendered when there is simulation state associated with it.
        /// </summary>
        protected override bool ShouldRender() => this.HasState;
    }
}