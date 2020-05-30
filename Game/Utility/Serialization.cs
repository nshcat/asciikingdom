using System.IO;
using System.Text.Json;

namespace Game.Utility
{
    /// <summary>
    /// Class containing serialization helpers
    /// </summary>
    public static class Serialization
    {
        /// <summary>
        /// Serialize object to JSON file at given path
        /// </summary>
        public static void SerializeToFile<T>(T obj, string destinationPath, JsonSerializerOptions options = null)
        {
            var text = JsonSerializer.Serialize<T>(obj, options);
            File.WriteAllText(destinationPath, text);
        }
        
        /// <summary>
        /// Deserialize object from JSON file at given path
        /// </summary>
        public static T DeserializeFromFile<T>(string sourcePath, JsonSerializerOptions options = null)
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(sourcePath), options);
        }
    }
}