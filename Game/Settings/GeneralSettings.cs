using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Settings
{
    /// <summary>
    /// Class containing general settings, such as tile sets, window size etc.
    /// </summary>
    public class GeneralSettings
    {
        /// <summary>
        /// Tileset used for the graphical game views and the border of the game.
        /// </summary>
        public string GraphicsTileset { get; set; }
            = "myne_rect.png";

        /// <summary>
        /// Scaling factor to use for the graphics tile set
        /// </summary>
        public float GraphicsTilesetScale { get; set; }
            = 1.0f;

        /// <summary>
        /// Tileset used for text portions of the game, such as menus.
        /// </summary>
        public string TextTileset { get; set; }
            = "VGA9x16.png";

        /// <summary>
        /// Scaling factor to use for the text tile set
        /// </summary>
        public float TextTilesetScale { get; set; }
            = 1.0f;

        /// <summary>
        /// Initial width of the game window, in pixels.
        /// </summary>
        public int GameWindowWidth { get; set; }
            = 1600;

        /// <summary>
        /// Initial height of the game window, in pixels.
        /// </summary>
        public int GameWindowHeight { get; set; }
            = 860;
    }
}
