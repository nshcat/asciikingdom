using Engine.Graphics;
using OpenToolkit.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Game.Serialization;

namespace Game.Simulation.Sites.Modules
{
    [SiteModuleId("fixeddrawer")]
    public class FixedSiteDrawer
        : SiteDrawer
    {
        #region Properties
        public override Tile Tile => this._tile;
        #endregion

        #region Fields
        private Tile _tile = new Tile(0);
        #endregion

        public FixedSiteDrawer(WorldSite site)
            : base(site)
        {

        }

        #region De/Serialization and Initialization
        public override void Initialize(SiteDeserializationHelper helper)
        {
            base.Initialize(helper);

            if(helper.HasSubkey("tile"))
            {
                var tileObj = helper.GetObject("tile");
                this._tile = JsonSerializer.Deserialize<Tile>(tileObj.Node, Serialization.Serialization.DefaultOptions);
            }
        }

        public override void Serialize(SiteSerializationHelper helper)
        {
            base.Serialize(helper);
            var tileObj = JsonSerializer.SerializeToNode(this._tile, Serialization.Serialization.DefaultOptions);
            helper.AddNode("tile", tileObj);
        }

        public override void Deserialize(SiteDeserializationHelper helper)
        {
            base.Deserialize(helper);
            var tileObj = helper.GetObject("tile");
            this._tile = JsonSerializer.Deserialize<Tile>(tileObj.Node, Serialization.Serialization.DefaultOptions);
        }
        #endregion

        #region Game Logic Methods
        public override void Update(int weeks)
        {
            return;
        }
        #endregion
    }
}
