using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Settings
{
    /// <summary>
    /// Root settings class, containing all settings categories
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Settings category for general settings, such as tile sets, game window size etc.
        /// </summary>
        public GeneralSettings General { get; set; }
            = new GeneralSettings();
    }
}
