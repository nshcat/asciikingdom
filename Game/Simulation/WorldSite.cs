using System;
using System.Collections.Generic;
using System.Reflection;
using Engine.Core;
using Engine.Graphics;
using Game.Simulation.Modules;

namespace Game.Simulation
{
    /// <summary>
    /// Represents a special site on the world map, such as cities, villages and colonies.
    /// </summary>
    public abstract class WorldSite : IGameObject
    {
        #region Properties
        /// <summary>
        /// The name for this site, specified by the user
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The position of the site on the world map
        /// </summary>
        public Position Position { get; set; }
        
        /// <summary>
        /// The identifying GUID of this site
        /// </summary>
        public Guid Id { get; set; }
        #endregion
        
        #region Abstract Properties
        /// <summary>
        /// A descriptive string for the type of this world site, such as "City" or "Village"
        /// </summary>
        public abstract string TypeDescriptor { get; }

        /// <summary>
        /// The tile used to represent this site on the world map
        /// </summary>
        public abstract Tile Tile { get; }
        #endregion
        
        #region Module Management
        /// <summary>
        /// All modules currently associated with this site
        /// </summary>
        protected Dictionary<Type, SiteModule> Modules { get; }
            = new Dictionary<Type, SiteModule>();

        /// <summary>
        /// Return reference of module of given type, if any.
        /// </summary>
        /// <typeparam name="T">Type of module to retrieve</typeparam>
        /// <returns>Reference to module, if site contains module of requested type</returns>
        public T QueryModule<T>() where T : SiteModule
        {
            return this.QueryModule(typeof(T)) as T;
        }

        /// <summary>
        /// Return reference of module of given type, if any.
        /// </summary>
        /// <param name="type">Type of module to retrieve</param>
        /// <returns>Reference to module, if site contains module of requested type</returns>
        public SiteModule QueryModule(Type type)
        {
            if (this.Modules.ContainsKey(type))
                return this.Modules[type];
            else throw new ArgumentException($"Site does not contain module of type {type}");
        }

        /// <summary>
        /// Check whether this site currently contains a module of given type.
        /// </summary>
        /// <typeparam name="T">Type of module to check for</typeparam>
        /// <returns>Flag indicating whether the site currently contains a module of given type</returns>
        public bool HasModule<T>() where T : SiteModule
        {
            return this.HasModule(typeof(T));
        }

        /// <summary>
        /// Check whether this site currently contains a module described by given type value.
        /// </summary>
        /// <param name="type">The type value corresponding to the type of module to check for</param>
        /// <returns>Flag indicating whether a module of given type is part of this site</returns>
        public bool HasModule(Type type)
        {
            return this.Modules.ContainsKey(type);
        }

        /// <summary>
        /// Add given module to this site.
        /// </summary>
        /// <param name="module">Module to add to this site.</param>
        public void AddModule<T>(T module) where T : SiteModule
        {
            if(this.HasModule<T>())
                throw new ArgumentException($"Site already contains module of type {typeof(T)}");
            
            this.Modules.Add(typeof(T), module);
        }

        /// <summary>
        /// Remove module of given type, if any.
        /// </summary>
        /// <remarks>
        /// This method will throw if no module of given type currently exists in this site.
        /// </remarks>
        /// <typeparam name="T">Type of module to remove</typeparam>
        public void RemoveModule<T>() where T : SiteModule
        {
            if (this.HasModule<T>())
                this.Modules.Remove(typeof(T));
            else throw new ArgumentException($"Site does not contain module of type {typeof(T)}");
        }
        #endregion
        
        /// <summary>
        /// Base class constructor. Sets a new GUID.
        /// </summary>
        public WorldSite(string name, Position position)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
            this.Position = position;
        }
        
        /// <summary>
        /// Update the state of this game object based on the given number of elapsed weeks.
        /// </summary>
        /// <param name="weeks">Number of weeks elapsed since last update</param>
        public abstract void Update(int weeks);
    }
}