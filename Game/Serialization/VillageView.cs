using Engine.Core;
using Game.Simulation;
using Game.Simulation.Modules;

namespace Game.Serialization
{
    /// <summary>
    /// Serialization view for <see cref="Village"/>
    /// </summary>
    public class VillageView : SiteView
    {
        public int Population { get; set; }
        
        public Village MakeObject()
        {
            var village = new Village(this.Name, this.Position, this.Population, null);
            village.Id = this.Id;
            return village;
        }
    }
}