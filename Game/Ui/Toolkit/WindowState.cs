using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Ui.Toolkit
{
    /// <summary>
    /// Class storing state of currently active window
    /// </summary>
    public record WindowState
    {
        /// <summary>
        /// Whether the window is supposed to have a scroll bar.
        /// </summary>
        public bool HasScrollBar { get; set; }
            = false;

        /// <summary>
        /// The offset command. This is stored so we can change the y offset _after_ all controls have been
        /// created in the window (on EndWindow)
        /// </summary>
        public SetYOffsetCommand OffsetCommand { get; set; }

        /// <summary>
        /// Record of all selectable widget and their y position. Used to determine scrolling position.
        /// </summary>
        public List<(string, int)> WidgetPositions { get; set; }
            = new List<(string, int)>();
    }
}
