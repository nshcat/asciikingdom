using System;
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