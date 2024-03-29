﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Simulation.Sites
{
    /// <summary>
    /// Attribute used to mark a property inside a site class as a module that gets
    /// automatically registered
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ModuleDataAttribute : Attribute
    {
        /// <summary>
        /// JSON key used for this module data property
        /// </summary>
        public string Key { get; protected set; }

        /// <summary>
        /// The default value of the module data property
        /// </summary>
        public object DefaultValue { get; protected set; }

        /// <summary>
        /// Annotate property to be automatically registered as site module
        /// </summary>
        public ModuleDataAttribute(string key, object defValue = null)
        {
            this.Key = key;
            this.DefaultValue = defValue;
        }
    }

    /// <summary>
    /// Attribute used to define a site module type ID and associate it with a site module class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SiteModuleIdAttribute : Attribute
    {
        /// <summary>
        /// Id associated with this attribute, commonly in the form "sitemodule.[name]"
        /// </summary>
        public string Id { get; protected set; }

        /// <summary>
        /// Annotate site module class with a fixed site module type id supplied as string
        /// </summary>
        /// <param name="id">Id to associated with site module type, as string</param>
        public SiteModuleIdAttribute(string id)
        {
            Id = id;
        }
    }
}
