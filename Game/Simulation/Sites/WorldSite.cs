using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Nodes;
using Engine.Core;
using Engine.Graphics;
using Game.Serialization;

namespace Game.Simulation.Sites
{
    // Have WorldSiteFactory that creates new world sites based on json templte, loaded into worldsitedata?
    // Have initialize method? for sitemodules etc aswell (e.g. initial population)

    /// <summary>
    /// Represents a special site on the world map, such as cities, villages and colonies.
    /// </summary>
    public class WorldSite : IGameObject
    {
        #region Properties
        /// <summary>
        /// The site manager this site is registered with
        /// </summary>
        public SiteManager Manager { get; set; }

        /// <summary>
        /// The name for this site, specified by the user
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The identifying GUID of this site
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id of type class this site was generated from.
        /// </summary>
        public string TypeId { get; set; }

        /// <summary>
        /// A descriptive string for the type of this world site, such as "City" or "Village"
        /// </summary>
        public string TypeDescriptor { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Base class constructor. Sets a new GUID.
        /// </summary>
        public WorldSite(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        /// <summary>
        /// Empty constructor, used for deserialization
        /// </summary>
        public WorldSite()
        {
            Id = Guid.NewGuid();
            Name = "";
        }
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
            return QueryModule(typeof(T)) as T;
        }

        /// <summary>
        /// Return reference of module of given type, if any.
        /// </summary>
        /// <param name="type">Type of module to retrieve</param>
        /// <returns>Reference to module, if site contains module of requested type</returns>
        public SiteModule QueryModule(Type type)
        {
            if (Modules.ContainsKey(type))
                return Modules[type];
            else throw new ArgumentException($"Site does not contain module of type {type}");
        }

        /// <summary>
        /// Return first module that derives from given abstract site module type
        /// </summary>
        public T QueryAbstractModule<T>() where T : SiteModule
        {
            return this.QueryAbstractModule(typeof(T)) as T;
        }

        /// <summary>
        /// Return first module that derives from given abstract site module type
        /// </summary>
        public SiteModule QueryAbstractModule(Type abstractBaseType)
        {
            foreach(var kvp in this.Modules)
            {
                if (kvp.Key.IsSubclassOf(abstractBaseType))
                    return kvp.Value;
            }

            throw new ArgumentException($"Site does not contain a module derived from {abstractBaseType}");
        }

        /// <summary>
        /// Check whether a module exists that derives from given abstract site module type
        /// </summary>
        public bool HasAbstractModule<T>() where T : SiteModule
        {
            return this.HasAbstractModule(typeof(T));
        }

        /// <summary>
        /// Check whether a module exists that derives from given abstract site module type
        /// </summary>
        public bool HasAbstractModule(Type abstractBaseType)
        {
            foreach (var kvp in this.Modules)
            {
                if (kvp.Key.IsSubclassOf(abstractBaseType))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check whether this site currently contains a module of given type.
        /// </summary>
        /// <typeparam name="T">Type of module to check for</typeparam>
        /// <returns>Flag indicating whether the site currently contains a module of given type</returns>
        public bool HasModule<T>() where T : SiteModule
        {
            return HasModule(typeof(T));
        }

        /// <summary>
        /// Check whether this site currently contains a module described by given type value.
        /// </summary>
        /// <param name="type">The type value corresponding to the type of module to check for</param>
        /// <returns>Flag indicating whether a module of given type is part of this site</returns>
        public bool HasModule(Type type)
        {
            return Modules.ContainsKey(type);
        }

        /// <summary>
        /// Add given module to this site.
        /// </summary>
        /// <param name="module">Module to add to this site.</param>
        public void AddModule<T>(T module) where T : SiteModule
        {
            if (this.HasModule<T>())
                throw new ArgumentException($"Site already contains module of type {typeof(T)}");

            this.AddModuleDirect(typeof(T), module);
        }

        /// <summary>
        /// Add site module based on given type.
        /// </summary>
        public void AddModuleDirect(Type moduleType, SiteModule module)
        {
            if (this.HasModule(moduleType))
                throw new ArgumentException($"Site already contains module of type {moduleType}");

            this.Modules.Add(moduleType, module);
            this.Manager?.Notify(SiteNotificationType.ModuleAdded, this, moduleType);
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
            {
                Modules.Remove(typeof(T));
                this.Manager?.Notify(SiteNotificationType.ModuleRemoved, this, typeof(T));
            }
            else throw new ArgumentException($"Site does not contain module of type {typeof(T)}");
        }
        #endregion

        #region De/Serialization
        public void Serialize(SiteSerializationHelper helper)
        {
            // Write site properties
            helper.WriteGuid("id", this.Id);
            helper.WriteValue("name", this.Name);
            helper.WriteValue("type", this.TypeDescriptor);
            helper.WriteValue("typeid", this.TypeId);

            // Create module array
            var moduleArray = new JsonArray();

            foreach(var kvp in this.Modules)
            {
                var moduleNode = new SiteSerializationHelper(helper.Manager);

                // Retrieve and write out type code for module
                var typeId = helper.Manager.GetSiteModuleTypeIdFor(kvp.Key);
                moduleNode.WriteValue("type", typeId);

                // Serialize module state to JSON subobject
                var stateNode = moduleNode.WriteObject("state");
                kvp.Value.Serialize(stateNode as SiteSerializationHelper);

                moduleArray.Add(moduleNode.Node);
            }

            helper.Node.Add("modules", moduleArray);
        }

        public void Deserialize(SiteDeserializationHelper helper)
        {
            // Read site properties
            this.Id = helper.ReadGuid("id");
            this.Name = helper.ReadValue<string>("name");
            this.TypeDescriptor = helper.ReadValue<string>("type");
            this.TypeId = helper.ReadValue<string>("typeid");

            // Retrieve module array
            this.Modules.Clear();
            var moduleArray = helper.GetArray("modules");

            foreach (var moduleNode in moduleArray)
            {
                var moduleObj = new SiteDeserializationHelper(helper.Manager, moduleNode.AsObject());
                var typeId = moduleObj.ReadValue<string>("type");

                // Find corresponding component type
                var moduleType = helper.Manager.GetSiteModuleTypeFor(typeId);

                // Create module instance, this site is the only parameter.
                var moduleInstance = Activator.CreateInstance(moduleType, new object[] { this });
                var module = (SiteModule)moduleInstance;

                // Retrieve state JSON node and deserialize
                module.Deserialize(moduleObj.GetObject("state") as SiteDeserializationHelper);

                // Register module with this site
                this.Modules.Add(moduleType, module);
            }
        }

        /// <summary>
        /// Step after deserialization. Resolve site references etc.
        /// </summary>
        /// <param name="helper"></param>
        public void PostDeserialize(SiteDeserializationHelper helper)
        {
            // Delegate to modules
            foreach (var kvp in this.Modules)
                kvp.Value.PostDeserialize(helper);
            return;
        }
        #endregion

        /// <summary>
        /// Update the state of this game object based on the given number of elapsed weeks.
        /// </summary>
        /// <param name="weeks">Number of weeks elapsed since last update</param>
        public void Update(int weeks)
        {
            // Update all modules
            foreach(var kvp in this.Modules)
                kvp.Value.Update(weeks);
            return;
        } 
    }
}