using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Simulation.Sites.Modules
{
    /// <summary>
    /// Base class for site modules that describe how a site is supposed to be drawn in the world view
    /// </summary>
    public abstract class SiteDrawer
        : SiteModule
    {
        #region Properties
        /// <summary>
        /// Tile that represents the associated site
        /// </summary>
        public abstract Tile Tile { get; }

        /// <summary>
        /// Z level on which the tile of this site drawer will be rendered. Tiles with higher z level cover
        /// tiles with lower z level.
        /// </summary>
        public int ZLevel { get; set; }
            = 0;
        #endregion

        public SiteDrawer(WorldSite parentSite)
            : base(parentSite)
        {

        }

        #region De/Serialization and Initialization
        public override void Initialize(SiteDeserializationHelper helper)
        {
            this.ZLevel = helper.ReadValue<int>("zlevel", 0);
        }

        public override void Serialize(SiteSerializationHelper helper)
        {
            helper.WriteValue("zlevel", this.ZLevel);
        }

        public override void Deserialize(SiteDeserializationHelper helper)
        {
            this.ZLevel = helper.ReadValue<int>("zlevel");
        }
        #endregion
    }
}
