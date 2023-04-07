using Engine.Core;
using Engine.Graphics;
using Game.Core;
using Game.Simulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Ui
{
    /// <summary>
    /// Base class for UI elements that are overlaid over the map terrain view, such as
    /// helpers to show the player where certain crops may grow.
    /// </summary>
    public abstract class MapOverlay
    {
        /// <summary>
        /// Whether the map overlay should blink and periodically reveal the underlying map.
        /// This is especially useful for overlays that extensively cover the map (such as
        /// a crop fertility overview).
        /// </summary>
        public bool Blinking { get; set; }

        /// <summary>
        /// Return a tile for given world position.
        /// </summary>
        /// <param name="world">The current world instance</param>
        /// <param name="worldPosition">The world position to determine a tile for</param>
        public abstract Optional<Tile> DetermineTile(Map world, Position worldPosition);
    }
}
