using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json;
using NLog.LayoutRenderers.Wrappers;
using Game.Data;
using Engine.Core;
using System.Xml.Linq;

namespace Game.Simulation.Sites
{
    /// <summary>
    /// Class managing all sites that are part of the game state
    /// </summary>
    public class SiteManager
    {
        // TODO: Have caching structures for queries like "get all entities with component X"
        // => EventManager static class manages ComponentAdded and ComponentRemoved events etc, and Entity
        // class fires those global events

        #region Properties

        /// <summary>
        /// All sites managed by this object
        /// </summary>
        public Dictionary<Guid, WorldSite> Sites { get; protected set; }
            = new Dictionary<Guid, WorldSite>();

        /// <summary>
        /// All sites managed by this object, as a list.
        /// </summary>
        public List<WorldSite> AllSites => Sites.Values.ToList();

        /// <summary>
        /// Dictionary of all sites associated with their position
        /// </summary>
        public Dictionary<Position, WorldSite> AllSitesByPosition => this.Sites.Select(x => x.Value).ToDictionary(x => x.Position);

        #endregion

        #region Private Fields

        // Cache used to speed up site module type id extraction
        private Dictionary<Type, string> _siteModuleTypeToId
            = new Dictionary<Type, string>();

        // Associates site module type IDs with their type object
        private Dictionary<string, Type> _siteModuleIdToType
            = new Dictionary<string, Type>();

        #endregion

        #region Public Interface

        /// <summary>
        /// Create new entity manager instance
        /// </summary>
        public SiteManager()
        {
            DiscoverSiteModules();
        }

        /// <summary>
        /// Checks whether a site with given instance id exists.
        /// </summary>
        public bool HasSite(Guid id)
        {
            return this.Sites.ContainsKey(id);
        }

        /// <summary>
        /// Updates state of all sites
        /// </summary>
        public void Update(int weeks)
        {
            foreach(var kvp in this.Sites)
            {
                kvp.Value.Update(weeks);
            }
        }

        /// <summary>
        /// Create new site based on site template with given type
        /// </summary>
        public WorldSite CreateSite(string type, Position position)
        {
            // Retrieve type class for requested site type
            var siteTypeClass = SiteTypeManager.Instance.GetType(type);

            // First, create the empty site object and fill it with metadata
            var site = new WorldSite();
            site.Position = position;
            site.TypeId = siteTypeClass.Identifier;
            site.TypeDescriptor = siteTypeClass.TypeDescriptor;
            
            // Now build the modules
            foreach(var kvp in siteTypeClass.Modules)
            {
                // Find corresponding component type
                var moduleType = this.GetSiteModuleTypeFor(kvp.Key);

                // Create module instance, this site is the only parameter.
                var moduleInstance = Activator.CreateInstance(moduleType, new object[] { site });
                var module = (SiteModule)moduleInstance;

                // Perform initialization if an initialization block was given in the type class
                if(kvp.Value != null)
                    module.Initialize(new SiteDeserializationHelper(this, kvp.Value));

                site.AddModuleDirect(moduleType, module);
            }

            // Register site with this manager
            this.AddSite(site);
            return site;
        }

        /// <summary>
        /// Retrieves site with given instance id. Will throw if no such entity exists.
        /// </summary>
        public WorldSite GetSite(Guid id)
        {
            if (!this.HasSite(id))
                throw new ArgumentException($"No site exists with id {id}");

            return this.Sites[id];
        }

        /// <summary>
        /// Add new site to be managed by this class
        /// </summary>
        public void AddSite(WorldSite site)
        {
            if (this.HasSite(site.Id))
                throw new ArgumentException($"Site with id {site.Id} already exists");

            Sites.Add(site.Id, site);
        }

        /// <summary>
        /// Remove given site from the manager.
        /// </summary>
        public void RemoveSite(WorldSite site)
        {
            if (!this.HasSite(site.Id))
                throw new ArgumentException($"No site exists with id {site.Id}");

            this.Sites.Remove(site.Id);
        }

        /// <summary>
        /// Retrieve type id for the given site module type
        /// </summary>
        public string GetSiteModuleTypeIdFor(Type type)
        {
            if (!this._siteModuleTypeToId.ContainsKey(type))
                throw new ArgumentException($"Type {type} isnt a valid site module type or has no type id associated with");

            return this._siteModuleTypeToId[type];
        }

        /// <summary>
        /// Retrieve type id for the given site module
        /// </summary>
        public string GetSiteModuleTypeIdFor(SiteModule module)
        {
            return GetSiteModuleTypeIdFor(module.GetType());
        }

        /// <summary>
        /// Retrieve side module type corresponding to given type id
        /// </summary>
        public Type GetSiteModuleTypeFor(string typeId)
        {
            if (!this._siteModuleIdToType.ContainsKey(typeId))
                throw new ArgumentException($"Site module type id {typeId} is not known");

            return this._siteModuleIdToType[typeId];
        }

        /// <summary>
        /// Deserialize site manager state from given JSON node
        /// </summary>
        /// <param name="node"></param>
        public void Deserialize(JsonNode node)
        {
            try
            {
                // Node is supposed to be an array of json objects
                var array = node.AsArray();

                List<WorldSite> loadedSites = new List<WorldSite>();
                foreach (var innerNode in array)
                {
                    // We expect an object here
                    var innerObj = innerNode.AsObject();
                    var helper = new SiteDeserializationHelper(this, innerObj);
                    var site = new WorldSite();
                    site.Deserialize(helper);

                    if (this.Sites.ContainsKey(site.Id))
                        throw new JsonException($"Encountered duplicate world site id {site.Id}");

                    this.Sites.Add(site.Id, site);
                    loadedSites.Add(site);
                }

                // Now, perform post-deserizalization work on all sites
                for(int idx = 0; idx < array.Count; ++idx)
                {
                    var innerObj = array[idx].AsObject();
                    var site = loadedSites[idx];
                    var helper = new SiteDeserializationHelper(this, innerObj);
                    site.PostDeserialize(helper);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to deserialize entity manager: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Serialize site manager into a JSON node
        /// </summary>
        public JsonNode Serialize()
        {
            var array = new JsonArray();

            // Serialize all registered sites
            foreach(var kvp in this.Sites)
            {
                var helper = new SiteSerializationHelper(this);

                // Serialize site into it
                kvp.Value.Serialize(helper);

                // Add to array
                array.Add(helper.Node);
            }

            return array;
        }

        #endregion

        #region Internal Methods
        /// <summary>
        /// Retrieve the type id from given site module type
        /// </summary>
        private string RetrieveSiteModuleTypeId(Type moduleType)
        {
            // Try to extract it from attributes
            var attribs = moduleType.GetCustomAttributes(typeof(SiteModuleIdAttribute), true);

            // There has to be at least one attribute here
            if (attribs.Length <= 0)
                throw new ArgumentException($"Site module type {moduleType} does not have a type id attribute applied to it");

            // Just take the first one
            var id = (attribs[0] as SiteModuleIdAttribute).Id;

            // Update the cache
            _siteModuleTypeToId.Add(moduleType, id);

            return id;
        }

        /// <summary>
        /// Discovers all site modules and records their type information and type ID for
        /// later activation during deserialization
        /// </summary>
        private void DiscoverSiteModules()
        {
            var baseClassType = typeof(SiteModule);
            var ns = "Game.Simulation.Sites.Modules";
            var types = from type in Assembly.GetExecutingAssembly().GetTypes()
                        where type.IsClass && type.Namespace == ns && type.IsSubclassOf(baseClassType) && !type.IsAbstract
                        select type;

            // Extract ID attribute
            foreach (var type in types)
            {
                var typeId = this.RetrieveSiteModuleTypeId(type);
                _siteModuleIdToType[typeId] = type;
                _siteModuleTypeToId[type] = typeId;
            }
        }

        #endregion
    }
}
