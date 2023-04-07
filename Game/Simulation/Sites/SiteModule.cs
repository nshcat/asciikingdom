using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Game.Serialization;

namespace Game.Simulation.Sites
{
    /// <summary>
    /// Base class for modules, which can be used to implement functionality of sites
    /// </summary>
    public abstract class SiteModule
    {
        // TODO automatic init/de/serialization using attributes (like, can be inited etc etc)

        #region Properties
        /// <summary>
        /// The site this module is associated with
        /// </summary>
        protected WorldSite ParentSite { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Base class constructor
        /// </summary>
        /// <param name="parentSite">The parent site this module is associated with</param>
        public SiteModule(WorldSite parentSite)
        {
            ParentSite = parentSite;
        }
        #endregion

        #region Game Logic Methods
        /// <summary>
        /// Perform logic update based on given amount of elapsed weeks since last update
        /// </summary>
        /// <param name="weeks">Number of weeks elapsed since last update</param>
        public abstract void Update(int weeks);
        #endregion

        #region De/Serialization and Initialization
        /// <summary>
        /// Initialize site module from template json data
        /// </summary>
        /// <param name="helper"></param>
        public abstract void Initialize(SiteDeserializationHelper helper);

        /// <summary>
        /// Serialize site module state to JSON
        /// </summary>
        public abstract void Serialize(SiteSerializationHelper helper);

        /// <summary>
        /// Deserialize site module state from JSON
        /// </summary>
        /// <param name="helper"></param>
        public abstract void Deserialize(SiteDeserializationHelper helper);

        /// <summary>
        /// Perform post-deserialization tasks
        /// </summary>
        /// <param name="helper"></param>
        public virtual void PostDeserialize(SiteDeserializationHelper helper)
        {
            return;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Return reference of module of given type, if any.
        /// </summary>
        /// <typeparam name="T">Type of module to retrieve</typeparam>
        /// <returns>Reference to module, if site contains module of requested type</returns>
        public T QueryModule<T>() where T : SiteModule
        {
            return QueryModule(typeof(T)) as T;
        }

        /// <summary>
        /// Return reference of module of given type, if any.
        /// </summary>
        /// <param name="type">Type of module to retrieve</param>
        /// <returns>Reference to module, if site contains module of requested type</returns>
        public SiteModule QueryModule(Type type)
        {
            return ParentSite.QueryModule(type);
        }

        /// <summary>
        /// Check whether the site this module belongs to currently contains a module of given type.
        /// </summary>
        /// <typeparam name="T">Type of module to check for</typeparam>
        /// <returns>Flag indicating whether the site currently contains a module of given type</returns>
        public bool HasModule<T>() where T : SiteModule
        {
            return HasModule(typeof(T));
        }

        /// <summary>
        /// Check whether the site this module belongs to  contains a module described by given type value.
        /// </summary>
        /// <param name="type">The type value corresponding to the type of module to check for</param>
        /// <returns>Flag indicating whether a module of given type is part of this site</returns>
        public bool HasModule(Type type)
        {
            return ParentSite.HasModule(type);
        }
        #endregion
    }
}