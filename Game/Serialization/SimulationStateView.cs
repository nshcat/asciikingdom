using System.Collections.Generic;
using System.Linq;
using Game.Simulation;

namespace Game.Serialization
{
    /// <summary>
    /// Serialization view for <see cref="SimulationState"/>
    /// </summary>
    public class SimulationStateView
    {
        public List<ProvinceView> Provinces { get; set; }
        
        public Date Date { get; set; }

        public SimulationState MakeObject(World world)
        {
            var provinces = this.Provinces.Select(x => x.MakeObject()).ToList();
            var simulation = new SimulationState(world);
            simulation.Provinces = provinces;
            simulation.Date = this.Date;
            return simulation;
        }
    }
}