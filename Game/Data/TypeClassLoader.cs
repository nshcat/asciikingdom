using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Engine.Resources;

namespace Game.Data
{
    /// <summary>
    /// Base class that implements type class loading from JSON data files
    /// </summary>
    /// <typeparam name="T">The type class that will be loaded</typeparam>
    public abstract class TypeClassLoader<T> where T : ITypeClass
    {
        /// <summary>
        /// The name of the file containing the type class data
        /// </summary>
        protected string FileName { get; set; }
        
        /// <summary>
        /// Collection of all known type classes
        /// </summary>
        protected Dictionary<string, T> Types { get; set; }

        /// <summary>
        /// Public access point to the collection of known type classes. It does not allow modification.
        /// </summary>
        public IReadOnlyDictionary<string, T> AllTypes => this.Types;

        /// <summary>
        /// Construct a new, empty type class loader.
        /// </summary>
        /// <param name="filename">The name of the JSON file to load data from. This can contain subfolders.</param>
        public TypeClassLoader(string filename)
        {
            this.FileName = filename;
            this.Types = new Dictionary<string, T>();
        }

        /// <summary>
        /// Load all type classes from JSON file.
        /// </summary>
        public void LoadTypes(ResourceManager resourceManager)
        {
            var contents = resourceManager.GetJSON(this.FileName);
            var types = JsonSerializer.Deserialize<List<T>>(contents, Serialization.Serialization.DefaultOptions);

            foreach (var type in types)
            {
                if (this.Types.ContainsKey(type.Identifier))
                {
                    throw new DataException($"Type class identifier was not unique: {type.Identifier}");
                }
                
                this.Types.Add(type.Identifier, type);
            }
        }

        /// <summary>
        /// Retrieve type class instance by identifier
        /// </summary>
        /// <param name="identifier">Unique identifier of the requested type class instance</param>
        public T GetType(string identifier)
        {
            if(!this.Types.ContainsKey(identifier))
                throw new ArgumentException($"Unknown type class identifier: {identifier}");

            return this.Types[identifier];
        }
    }
}