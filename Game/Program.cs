using System;
using System.Text.Json;
using Game.Core;
using Game.WorldGen;

namespace Game
{
    static class Program
    {
        static void Main(string[] args)
        {
            GameDirectories.EnsureDirectories();
            new Game().Run();
        }
    }
}