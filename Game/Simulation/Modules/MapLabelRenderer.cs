using System;

namespace Game.Simulation.Modules
{
    /// <summary>
    /// Enumeration describing the different map label rendering styles
    /// </summary>
    public enum MapLabelStyle
    {
        /// <summary>
        /// Just the site name
        /// </summary>
        Normal,
        
        /// <summary>
        /// Site name, surrounded by two stars. Used for capitals
        /// </summary>
        Capital
    }
    
    /// <summary>
    /// Module that renders the map label for the associated site
    /// </summary>
    public class MapLabelRenderer : SiteModule
    {
        /// <summary>
        /// The current map label style used to render the label
        /// </summary>
        public MapLabelStyle LabelStyle { get; set; }

        /// <summary>
        /// The map label for the associated site
        /// </summary>
        public string Label => this.MakeLabel();
        
        /// <summary>
        /// Create new map label renderer instance
        /// </summary>
        /// <param name="parentSite">Site this module is associated with</param>
        /// <param name="style">The style used to render the map label for the associated site</param>
        public MapLabelRenderer(WorldSite parentSite, MapLabelStyle style)
            : base(parentSite)
        {
            this.LabelStyle = style;
        }
        
        public override void Update(int weeks)
        {
            // Do nothing
        }

        /// <summary>
        /// Create the site label based on the current map label style.
        /// </summary>
        protected string MakeLabel()
        {
            switch (this.LabelStyle)
            {
                case MapLabelStyle.Capital:
                {
                    var marker = (char) 15;
                    return $"{marker}{this.ParentSite.Name}{marker}";
                }
                default:
                    return this.ParentSite.Name;
            }
        }
    }
}