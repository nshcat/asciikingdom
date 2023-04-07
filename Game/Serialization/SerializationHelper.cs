using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using Engine.Core;
using Game.Simulation.Sites;

namespace Game.Serialization
{
    /// <summary>
    /// Helper class that provides easy interface for component serialization
    /// </summary>
    public class SerializationHelper
    {
        /// <summary>
        /// JSON node to store attributes 
        /// </summary>
        public JsonObject Node { get; protected set; }

        /// <summary>
        /// Create new serialization helper instance with empty json object
        /// </summary>
        public SerializationHelper()
        {
            Node = new JsonObject();
        }

        /// <summary>
        /// Create new serialization helper instance with given json object as its node
        /// </summary>
        /// <param name="node"></param>
        protected SerializationHelper(JsonObject node)
        {
            Node = node;
        }

        /// <summary>
        /// Store given key value pair, where the value is a basic type
        /// </summary>
        public void WriteValue<T>(string key, T value)
        {
            Node.Add(key, JsonValue.Create(value));
        }

        /// <summary>
        /// Store given guid with given key
        /// </summary>
        public void WriteGuid(string key, Guid value)
        {
            Node.Add(key, value.ToString());
        }

        /// <summary>
        /// Store given position with given key
        /// </summary>
        public void WritePosition(string key, Position pos)
        {
            Node.Add(key, $"{pos.X};{pos.Y}");
        }

        /// <summary>
        /// Store array of basic values
        /// </summary>
        public void WriteValueArray<T>(string key, T[] values)
        {
            var array = new JsonArray();

            foreach (var value in values)
                array.Add(value);

            Node.Add(key, array);
        }

        /// <summary>
        /// Serialize reference to site using the sites unique ID
        /// </summary>
        public void WriteSiteReference(string key, WorldSite site)
        {
            Node.Add(key, JsonValue.Create(site.Id.ToString()));
        }

        /// <summary>
        /// Create and insert a new JSON sub object with given name
        /// </summary>
        /// <param name="key">Name of the new JSON sub object</param>
        /// <returns>New <see cref="SerializationHelper"/> instance referencing the newly created sub object</returns>
        public virtual SerializationHelper WriteObject(string key)
        {
            var subObject = new JsonObject();
            Node.Add(key, subObject);

            return new SerializationHelper(subObject);
        }

        /// <summary>
        /// Add given json object to this node
        /// </summary>
        public void AddObject(string key, JsonObject obj)
        {
            this.Node.Add(key, obj);
        }

        /// <summary>
        /// Add given json node to this node
        /// </summary>
        public void AddNode(string key, JsonNode node)
        {
            this.Node.Add(key, node);
        }
    }
}
