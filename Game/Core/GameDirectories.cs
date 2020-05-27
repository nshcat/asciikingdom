using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Game.Core
{
    /// <summary>
    /// Static class managing all directories relevant to the game, such as save game paths etc.
    /// </summary>
    public static class GameDirectories
    {
        /// <summary>
        /// Root folder for game user data, such as save games
        /// </summary>
        public static string UserData => Path.Combine(Root, "asciikingdom");

        /// <summary>
        /// Folder for saved games
        /// </summary>
        public static string SaveGames => Path.Combine(UserData, "saves");

        /// <summary>
        /// The local application data root folder
        /// </summary>
        private static string Root => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
            Environment.SpecialFolderOption.Create);


        /// <summary>
        /// Whether there is at least one saved game.
        /// </summary>
        //public static bool HasSaveGames =>

        /// <summary>
        /// Ensure that game directories have been created.
        /// </summary>
        public static void EnsureDirectories()
        {
            Directory.CreateDirectory(UserData);
            Directory.CreateDirectory(SaveGames);
        }
    }
}