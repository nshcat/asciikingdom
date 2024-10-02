using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Game.Serialization;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
using Engine.Core;

namespace Game.Simulation.Sites
{
    /// <summary>
    /// Base class for modules, which can be used to implement functionality of sites
    /// </summary>
    public abstract class SiteModule
    {
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
        public virtual void Initialize(SiteDeserializationHelper helper)
        {
            this.AutoInitialize(helper);
        }

        /// <summary>
        /// Serialize site module state to JSON
        /// </summary>
        public virtual void Serialize(SiteSerializationHelper helper)
        {
            this.AutoSerialize(helper);
        }

        /// <summary>
        /// Deserialize site module state from JSON
        /// </summary>
        /// <param name="helper"></param>
        public virtual void Deserialize(SiteDeserializationHelper helper)
        {
            this.AutoDeserialize(helper);
        }

        /// <summary>
        /// Perform post-deserialization tasks
        /// </summary>
        /// <param name="helper"></param>
        public virtual void PostDeserialize(SiteDeserializationHelper helper)
        {

            this.AutoPostDeserialize(helper);
        }
        #endregion

        #region Auto De/Serialization based on Attributes
        protected IEnumerable<(PropertyInfo, ModuleDataAttribute)> GetAnnotatedProperties()
        {
            var props = this.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(ModuleDataAttribute), false));
            var results = new List<(PropertyInfo, ModuleDataAttribute)>();
            foreach(var p in props)
            {
                results.Add((p, p.GetCustomAttributes(typeof(ModuleDataAttribute), false)[0] as ModuleDataAttribute));
            }
            return results;
        }

        /// <summary>
        /// Perform automatic deserialization of module data based on property attributes
        /// </summary>
        protected void AutoSerialize(SiteSerializationHelper helper)
        {
            foreach(var (prop, attribute) in this.GetAnnotatedProperties())
            {
                var val = prop.GetValue(this);
                var ty = prop.PropertyType;
                if(val is int || val is float || val is double || val is string || val is bool)
                {
                    var mi = helper.GetType().GetMethod("WriteValue");
                    mi.MakeGenericMethod(ty).Invoke(helper, new object[] { attribute.Key, val });
                }
                else if(ty.IsEnum)
                {
                    var mi = helper.GetType().GetMethod("WriteEnum");
                    mi.MakeGenericMethod(ty).Invoke(helper, new object[] { attribute.Key, val });
                }
                else if(val is Guid)
                {
                    helper.WriteGuid(attribute.Key, (Guid)val);
                }
                else if(val is Position)
                {
                    helper.WritePosition(attribute.Key, (Position)val);
                }
                else if (val is WorldSite site)
                {
                    // World sites are serialized as their GUID, which is resolved during post deserialization.
                    helper.WriteGuid(attribute.Key, site.Id);
                }
                else
                {
                    throw new Exception($"Annotated site module data property {prop.Name} has incompatible type {ty}");
                }
            }
        }

        protected void AutoDeserialize(SiteDeserializationHelper helper)
        {
            foreach (var (prop, attribute) in this.GetAnnotatedProperties())
            {
                var ty = prop.PropertyType;
                if (ty == typeof(int) || ty == typeof(float) || ty == typeof(double)
                    || ty == typeof(string) || ty == typeof(bool))
                {
                    var mi = helper.GetType().GetMethod("ReadValue");
                    var result = mi.MakeGenericMethod(ty).Invoke(helper, new object[] { attribute.Key, attribute.DefaultValue });
                    prop.SetValue(this, result);
                }
                else if (ty.IsEnum)
                {
                    var mi = helper.GetType().GetMethod("ReadEnum");
                    var result = mi.MakeGenericMethod(ty).Invoke(helper, new object[] { attribute.Key, attribute.DefaultValue });
                    prop.SetValue(this, result);
                }
                else if (ty == typeof(Guid))
                {
                    prop.SetValue(this, helper.ReadGuid(attribute.Key));
                }
                else if (ty == typeof(Position))
                {
                    prop.SetValue(this, helper.ReadPosition(attribute.Key));
                }
                else if (ty == typeof(WorldSite))
                {
                    // Ignore. It will be deserialized during post deserialization.
                }
                else
                {
                    throw new Exception($"Annotated site module data property {prop.Name} has incompatible type {ty}");
                }
            }
        }

        protected void AutoPostDeserialize(SiteDeserializationHelper helper)
        {
            foreach (var (prop, attribute) in this.GetAnnotatedProperties())
            {
                var ty = prop.PropertyType;
                // Perform world site resolution based on the GUID stored in the JSON.
                if (ty == typeof(WorldSite))
                {
                    var guid = helper.ReadValue<string>(attribute.Key);
                    prop.SetValue(this, helper.ResolveSiteReference(guid));
                }
            }
        }

        protected void AutoInitialize(SiteDeserializationHelper helper)
        {
            foreach (var (prop, attribute) in this.GetAnnotatedProperties())
            {
                var ty = prop.PropertyType;
                if (ty == typeof(int) || ty == typeof(float) || ty == typeof(double)
                    || ty == typeof(string) || ty == typeof(bool))
                {
                    var mi = helper.GetType().GetMethod("ReadValue");
                    var result = mi.MakeGenericMethod(ty).Invoke(helper, new object[] { attribute.Key, attribute.DefaultValue });
                    prop.SetValue(this, result);
                }
                else if (ty == typeof(Position))
                {
                    if(helper.HasSubkey(attribute.Key))
                    {
                        prop.SetValue(this, helper.ReadPosition(attribute.Key));
                    }
                    else
                    {
                        prop.SetValue(this, Position.Origin);
                    }                  
                }
                else if (ty.IsEnum)
                {
                    var mi = helper.GetType().GetMethod("ReadEnum");
                    var result = mi.MakeGenericMethod(ty).Invoke(helper, new object[] { attribute.Key, attribute.DefaultValue });
                    prop.SetValue(this, result);
                }
                else if (ty == typeof(Guid))
                {
                    // Dont use default values for GUIDs, that would not make sense
                    if (helper.HasSubkey(attribute.Key))
                        prop.SetValue(this, helper.ReadGuid(attribute.Key));
                    else
                        prop.SetValue(this, Guid.Empty);
                }
                else
                {
                    throw new Exception($"Annotated site module data property {prop.Name} has incompatible type {ty}");
                }
            }
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