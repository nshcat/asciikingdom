using System;
using Game.Serialization;

namespace Game.Simulation.Sites.Modules
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
    [SiteModuleId("maplabelrenderer")]
    public class MapLabelRenderer : SiteModule
    {
        #region Properties
        /// <summary>
        /// The current map label style used to render the label
        /// </summary>
        public MapLabelStyle LabelStyle { get; set; }
            = MapLabelStyle.Normal;

        /// <summary>
        /// The map label for the associated site
        /// </summary>
        public string Label => MakeLabel();
        #endregion

        #region Constructors
        /// <summary>
        /// Create new map label renderer instance
        /// </summary>
        /// <param name="parentSite">Site this module is associated with</param>
        /// <param name="style">The style used to render the map label for the associated site</param>
        public MapLabelRenderer(WorldSite parentSite, MapLabelStyle style)
            : base(parentSite)
        {
            LabelStyle = style;
        }

        public MapLabelRenderer(WorldSite parentSite)
            : base(parentSite)
        {

        }
        #endregion

        #region Game Logic Methods
        public override void Update(int weeks)
        {
            // Do nothing
        }
        #endregion

        #region De/Serialization Methods
        public override void Serialize(SiteSerializationHelper helper)
        {
            helper.WriteEnum("style", this.LabelStyle);
        }

        public override void Deserialize(SiteDeserializationHelper helper)
        {
            this.LabelStyle = helper.ReadEnum<MapLabelStyle>("style");
        }

        public override void Initialize(SiteDeserializationHelper helper)
        {
            this.LabelStyle = helper.ReadEnum<MapLabelStyle>("style", MapLabelStyle.Normal);
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Create the site label based on the current map label style.
        /// </summary>
        protected string MakeLabel()
        {
            switch (LabelStyle)
            {
                case MapLabelStyle.Capital:
                    {
                        var marker = (char)15;
                        return $"{marker}{ParentSite.Name}{marker}";
                    }
                default:
                    return ParentSite.Name;
            }
        }
        #endregion
    }
}