using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Linq;
using Engine.Core;
using Game.Simulation.Sites;

namespace Game.Serialization
{
    /// <summary>
    /// Helper class that provides easy interface for deserialization
    /// </summary>
    public class DeserializationHelper
    { 
        /// <summary>
        /// JSON node to load attributes from
        /// </summary>
        public JsonObject Node { get; protected set; }

        /// <summary>
        /// Create new serialization helper instance
        /// </summary>
        /// <param name="manager">Reference to the entity manager</param>
        public DeserializationHelper(JsonObject node)
        {
            Node = node;
        }  

        /// <summary>
        /// Get subarray with given key
        /// </summary>
        public JsonArray GetArray(string key)
        {
            JsonNode node;
            var success = Node.TryGetPropertyValue(key, out node);
            if (!success)
                throw new ArgumentException($"No array with key \"{key}\" exists");

            try
            {
                return node.AsArray();
            }
            catch (Exception)
            {
                throw new ArgumentException($"No array with key \"{key}\" exists");
            }
        }

        /// <summary>
        /// Get subarray with given key
        /// </summary>
        public virtual DeserializationHelper GetObject(string key)
        {
            JsonNode node;
            var success = Node.TryGetPropertyValue(key, out node);
            if (!success)
                throw new ArgumentException($"No object with key \"{key}\" exists");

            try
            {
                return new DeserializationHelper(node.AsObject());
            }
            catch (Exception)
            {
                throw new ArgumentException($"No object with key \"{key}\" exists");
            }
        }

        /// <summary>
        /// Read value corresponding to given key name. If no such value exists, the default is returned instead.
        /// </summary>
        public T ReadValue<T>(string key, T defaultValue = default)
        {
            if (!Node.ContainsKey(key))
                return defaultValue;

            JsonNode node;
            var success = Node.TryGetPropertyValue(key, out node);
            if (!success)
                return defaultValue;

            // Try to extract value
            try
            {
                return node.GetValue<T>();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Reads GUID value corresponding to the given key name.
        /// </summary>
        public Guid ReadGuid(string key)
        {
            if (!Node.ContainsKey(key))
                throw new ArgumentException($"Couldnt load GUID with key \"{key}\"");

            var strValue = ReadValue<string>(key);

            Guid id;
            var success = Guid.TryParse(strValue, out id);
            if (!success)
                throw new JsonException($"Couldnt pass Guid string value \"{strValue}\" for key \"{key}\"");

            return id;
        }

        /// <summary>
        /// Reads position value corresponding to the given key name
        /// </summary>
        public Position ReadPosition(string key)
        {
            if (!Node.ContainsKey(key))
                throw new ArgumentException($"Couldnt load GUID with key \"{key}\"");

            var strValue = ReadValue<string>(key);
            string[] parts = strValue.Split(new char[] { ';' });
            if (parts.Length != 2)
                throw new JsonException($"Failed to parse value '{strValue}' as position for key {key}");

            int x, y;
            if (!int.TryParse(parts[0], out x))
                throw new JsonException($"Failed to parse value '{strValue}' as position for key {key}");

            if (!int.TryParse(parts[1], out y))
                throw new JsonException($"Failed to parse value '{strValue}' as position for key {key}");

            return new Position(x, y);
        }

        /// <summary>
        /// Reads all values in the array corresponding to the given key name. On error, an empty array is returned.
        /// </summary>
        public T[] ReadValueArray<T>(string key)
        {
            if (!Node.ContainsKey(key))
                return new T[0];

            JsonNode node;
            var success = Node.TryGetPropertyValue(key, out node);
            if (!success)
                return new T[0];

            try
            {
                var array = node.AsArray();
                var results = new List<T>();
                foreach (var valueNode in array)
                {
                    try
                    {
                        results.Add(valueNode.GetValue<T>());
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                return results.ToArray();
            }
            catch (Exception)
            {
                return new T[0];
            }
        }

        /// <summary>
        /// Check whether a subkey with given name exists
        /// </summary>
        public bool HasSubkey(string key)
        {
            return this.Node.ContainsKey(key);
        }

        /// <summary>
        /// Check if given subkey exists in the underlying JSON object
        /// </summary>
        /// <param name="key">Sub key name to check</param>
        protected void CheckSubKey(string key)
        {
            if (!Node.ContainsKey(key))
                throw new ArgumentException($"JSON object did not contain key \"{key}\"");
        }
    }
}
