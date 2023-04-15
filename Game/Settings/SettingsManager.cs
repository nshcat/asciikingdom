using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Game.Core;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Game.Settings
{
    /// <summary>
    /// Class managing the active settings instance, and loading and saving of settings.
    /// </summary>
    public class SettingsManager
    {
        #region Properties
        /// <summary>
        /// The current settings instance, initialized to the default settings.
        /// </summary>
        public Settings Settings { get; set; }
            = new Settings();

        /// <summary>
        /// Name of the settings file to use
        /// </summary>
        protected string SettingsFileName => "settings.yaml";
        #endregion

        #region Settings Loading and Saving
        /// <summary>
        /// Load settings from the settings file, if possible. If no such file
        /// exists, create a default settings file.
        /// </summary>
        public void LoadSettings()
        {
            var path = this.GetSettingsFilePath();

            // Check if the settings file exist.
            if(!File.Exists(path))
            {
                // The settings object is already initialized with the default settings.
                // Just write them to disk.
                this.SaveSettings();
            }
            else // It exists, now try to load it.
            {
                var contents = File.ReadAllText(path);

                // Build the deserializer we are going to use.
                // We want snake case naming convention here.
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();

                // Deserialize the yaml to our settings object
                this.Settings = deserializer.Deserialize<Settings>(contents);
            }
        }

        /// <summary>
        /// Save the current settings state to file.
        /// </summary>
        public void SaveSettings()
        {
            var path = this.GetSettingsFilePath();

            using (var file = File.CreateText(path))
            {
                // Build the deserializer we are going to use.
                // We want snake case naming convention here.
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();

                // Serialize the current settings state to yaml string
                serializer.Serialize(file, this.Settings);
            }  
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Build path to the settings file
        /// </summary>
        private string GetSettingsFilePath()
        {
            return Path.Combine(GameDirectories.UserData, this.SettingsFileName);
        }
        #endregion

        #region Static Singleton Interface
        /// <summary>
        /// The static singleton instance.
        /// </summary>
        private static SettingsManager _instance;

        /// <summary>
        /// Retrieve global instance of the settings manager
        /// </summary>
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SettingsManager();

                return _instance;
            }
        }
        #endregion
    }
}
