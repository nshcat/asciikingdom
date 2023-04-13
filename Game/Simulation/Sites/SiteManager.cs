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
using Game.Simulation.Sites.Modules;

namespace Game.Simulation.Sites
{
    /// <summary>
    /// Types of notifications sites can send to the site manager
    /// </summary>
    public enum SiteNotificationType
    {
        /// <summary>
        /// A module was added
        /// </summary>
        ModuleAdded,

        /// <summary>
        /// A module was removed
        /// </summary>
        ModuleRemoved
    }

    /// <summary>
    /// Class managing all sites that are part of the game state
    /// </summary>
    public class SiteManager
    {
        #region Properties
        /// <summary>
        /// All sites managed by this object
        /// </summary>
        public Dictionary<Guid, WorldSite> Sites { get; protected set; }
            = new Dictionary<Guid, WorldSite>();
        #endregion

        #region Private Fields
        #region Caches for module type lookups
        // Cache used to speed up site module type id extraction
        private Dictionary<Type, string> _siteModuleTypeToId
            = new Dictionary<Type, string>();

        // Associates site module type IDs with their type object
        private Dictionary<string, Type> _siteModuleIdToType
            = new Dictionary<string, Type>();
        #endregion

        #region Caches for site queries
        // Cache for site filter queries
        private Dictionary<SiteFilter, List<WorldSite>> _siteQueryCache
            = new Dictionary<SiteFilter, List<WorldSite>>();

        // Whether we currently have a valid cache containing sites organized by position
        private bool _hasSiteByPositionCache
            = false;

        // Cache for queries for all world sites that have a position module, organized by position
        private Dictionary<Position, WorldSite> _siteByPositionCache
            = new Dictionary<Position, WorldSite>();
        #endregion
        #endregion

        #region Constructor, Game Logic Update and Notifications
        /// <summary>
        /// Create new entity manager instance
        /// </summary>
        public SiteManager()
        {
            DiscoverSiteModules();
        }

        /// <summary>
        /// Called by child world sites to notify this manager of changes concerning site modules etc
        /// </summary>
        public void Notify(SiteNotificationType notificationType, WorldSite site, Type moduleType)
        {
            this.InvalidateCaches();
        }

        /// <summary>
        /// Updates state of all sites
        /// </summary>
        public void Update(int weeks)
        {
            foreach (var kvp in this.Sites)
            {
                kvp.Value.Update(weeks);
            }
        }
        #endregion

        #region Site Management Methods
        /// <summary>
        /// Checks whether a site with given instance id exists.
        /// </summary>
        public bool HasSite(Guid id)
        {
            return this.Sites.ContainsKey(id);
        }

        /// <summary>
        /// Create new site based on site template with given type
        /// </summary>
        public WorldSite CreateSite(string type)
        {
            // Retrieve type class for requested site type
            var siteTypeClass = SiteTypeManager.Instance.GetType(type);

            // First, create the empty site object and fill it with metadata
            var site = new WorldSite();
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
        /// Create new site based on site template with given type and position. This requires
        /// that the site type template includes a position component.
        /// </summary>
        public WorldSite CreateSiteAt(string type, Position position)
        {
            var site = this.CreateSite(type);
            site.QueryModule<SitePosition>().Position = position;
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
            site.Manager = this;
            this.InvalidateCaches();
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
        #endregion

        #region Site Querying and Filtering
        /// <summary>
        /// Retrieve collection of world sites satisfying given site filter
        /// </summary>
        public IReadOnlyList<WorldSite> QuerySites(SiteFilter filter)
        {
            // First, check we have a cache entry for this query.
            if(this._siteQueryCache.ContainsKey(filter))
            {
                // If so, just return that.
                return this._siteQueryCache[filter];
            }

            // Otherwise, we have to recompute the set of matching sites.
            var results = this.Sites.Where(x => filter.Matches(x.Value)).Select(x => x.Value).ToList();

            // Record in our cache
            this._siteQueryCache[filter] = results;

            return results;
        }

        /// <summary>
        /// Return all sites that have a position, organized by position for faster lookup
        /// </summary>
        public IReadOnlyDictionary<Position, WorldSite> QuerySitesWithPosition()
        {
            // If we have a valid cache, use that
            if(this._hasSiteByPositionCache)
            {
                return this._siteByPositionCache;
            }

            // We have to rebuild the cache
            this._siteByPositionCache =
                this.Sites
                    .Where(x => x.Value.HasModule<SitePosition>())
                    .Select(x => x.Value)
                    .ToDictionary(x => x.QueryModule<SitePosition>().Position);

            this._hasSiteByPositionCache = true;
            return this._siteByPositionCache;
        }
        #endregion

        #region De/Serialization
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
        /// Invalidate all caches. Called when sites change etc.
        /// </summary>
        private void InvalidateCaches()
        {
            this._siteQueryCache.Clear();

            this._hasSiteByPositionCache = false;
            this._siteByPositionCache.Clear();
        }

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
