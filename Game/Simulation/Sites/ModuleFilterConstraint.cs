using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Simulation.Sites
{
    /// <summary>
    /// Filter constraint that matches against the existence of a specific module type
    /// </summary>
    public class ModuleFilterConstraint
        : ISiteFilterConstraint
    {
        /// <summary>
        /// The type of module to match against
        /// </summary>
        public Type ModuleType { get; protected set; }

        /// <summary>
        /// Whether the module type is an abstract base type and we are matching against all
        /// sub types of it
        /// </summary>
        public bool IsAbstractType { get; protected set; }

        /// <summary>
        /// Construct filter constraint
        /// </summary>
        public ModuleFilterConstraint(Type moduleType, bool isAbstractType)
        {
            this.ModuleType = moduleType;
            this.IsAbstractType = isAbstractType;
        }

        /// <summary>
        /// Check if given site contains the requested modules.
        /// </summary>
        public bool Matches(WorldSite site)
        {
            if (this.IsAbstractType)
                return site.HasAbstractModule(this.ModuleType);
            else
                return site.HasModule(this.ModuleType);
        }

        /// <summary>
        /// Factory method to create a filter constraint for given generic site module type.
        /// </summary>
        public static ModuleFilterConstraint Create<T>() where T : SiteModule
        {
            return new ModuleFilterConstraint(typeof(T), false);
        }

        /// <summary>
        /// Factory method to create a filter constraint for given generic abstract site module base type.
        /// Will match all derived site module types.
        /// </summary>
        public static ModuleFilterConstraint CreateAbstract<T>() where T : SiteModule
        {
            return new ModuleFilterConstraint(typeof(T), true);
        }
    }
}
