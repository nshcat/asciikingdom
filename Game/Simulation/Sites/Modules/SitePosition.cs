using Engine.Core;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Game.Simulation.Sites.Modules
{
    /// <summary>
    /// Site module that describes a postion of a site.
    /// </summary>
    [SiteModuleId("position")]
    public class SitePosition
        : SiteModule
    {
        #region Properties
        /// <summary>
        /// The position of the site on the world map
        /// </summary>
        [ModuleData("position")]
        public Position Position { get; set; }
        #endregion

        #region Constructors
        public SitePosition(WorldSite site)
            : base(site)
        {

        }
        #endregion

        #region De/Serialization and Initialization
        #endregion

        #region Game Logic Methods
        public override void Update(int weeks)
        {
            return;
        }
        #endregion
    }
}
