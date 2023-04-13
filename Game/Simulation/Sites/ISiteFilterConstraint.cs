using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Simulation.Sites
{
    /// <summary>
    /// Interface for site filter constraints
    /// </summary>
    public interface ISiteFilterConstraint
    {
        /// <summary>
        /// Match filter constraint against given site.
        /// </summary>
        public abstract bool Matches(WorldSite site);
    }
}
