using System;
using System.Text.Json;
using System.Threading;
using Game.Core;
using Game.Simulation;
using Game.WorldGen;
using System.Collections.Generic;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog;
using Game.Settings;

namespace Game
{
    static class Program
    {
        static void Main(string[] args)
        {
            SetupLogging();
            GameDirectories.EnsureDirectories();

            // Load settings as early as possible, since we need to know how big the
            // game window is supposed to be.
            SettingsManager.Instance.LoadSettings();

            LogManager.GetCurrentClassLogger().Warn("Bla bla bla");
            LogManager.GetCurrentClassLogger().Info("Bla bla bla");
            LogManager.GetCurrentClassLogger().Error("Bla bla bla");

            new Game().Run();
        }

        /// <summary>
        /// Setup NLog for this application
        /// </summary>
        static void SetupLogging()
        {
            var config = new LoggingConfiguration();

            // Colored console target with only the level strings colored
            var logconsole = new ColoredConsoleTarget("logconsole");
            logconsole.RowHighlightingRules.Clear();
            logconsole.UseDefaultRowHighlightingRules = false;
            logconsole.WordHighlightingRules.Add(new ConsoleWordHighlightingRule("Warn", ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));
            logconsole.WordHighlightingRules.Add(new ConsoleWordHighlightingRule("Error", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));
            logconsole.WordHighlightingRules.Add(new ConsoleWordHighlightingRule("Fatal", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));

            // Setup logger message format
            var layout = new SimpleLayout("${time} ${pad:padding=6:inner=${level:uppercase=false}}: ${message}");
            logconsole.Layout = layout;

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);

            NLog.LogManager.Configuration = config;
        }
    }
}