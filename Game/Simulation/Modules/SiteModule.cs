namespace Game.Simulation.Modules
{
    /// <summary>
    /// Base class for modules, which can be used to extend the functionality of site classes
    /// </summary>
    public abstract class SiteModule
    {
        /// <summary>
        /// The site this module is associated with
        /// </summary>
        protected WorldSite ParentSite { get; set; }

        /// <summary>
        /// Base class constructor
        /// </summary>
        /// <param name="parentSite">The parent site this module is associated with</param>
        public SiteModule(WorldSite parentSite)
        {
            this.ParentSite = parentSite;
        }

        /// <summary>
        /// Perform logic update based on given amount of elapsed weeks since last update
        /// </summary>
        /// <param name="weeks">Number of weeks elapsed since last update</param>
        public abstract void Update(int weeks);
    }
}