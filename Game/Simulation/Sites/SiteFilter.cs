using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Simulation.Sites
{
    /// <summary>
    /// Class encapasulating a filter for world sites
    /// </summary>
    public class SiteFilter : IEnumerable
    {
        /// <summary>
        /// List of all constraints that make up this filter
        /// </summary>
        private List<ISiteFilterConstraint> _constraints
            = new List<ISiteFilterConstraint>();

        /// <summary>
        /// List of all constraints that make up this filter
        /// </summary>
        public IReadOnlyList<ISiteFilterConstraint> Constraints => this._constraints;

        /// <summary>
        /// Trivial constructor
        /// </summary>
        public SiteFilter()
        {

        }

        /// <summary>
        /// Checks whether the given site matches this site filter.
        /// </summary>
        public bool Matches(WorldSite site)
        {
            return this._constraints.All(constraint => constraint.Matches(site));
        }

        #region IEnumerable and Collection Implementation
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)_constraints).GetEnumerator();
        }

        public void Add(ISiteFilterConstraint constraint)
        {
            this._constraints.Add(constraint);
        }
        #endregion
    }
}
