using Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Ui.Toolkit
{
    /// <summary>
    /// Class encapsulating various state and layout variables used for drawing widgets in
    /// a theme
    /// </summary>
    public record WidgetDrawParams
    {
        /// <summary>
        /// The widget position
        /// </summary>
        public Position Position { get; set; }

        /// <summary>
        /// Whether the widget is currently selected
        /// </summary>
        public bool IsSelected { get; set; }
            = false;

        /// <summary>
        /// Whether the widget is currently enabled
        /// </summary>
        public bool IsEnabled { get; set; }
            = true;

        /// <summary>
        /// Primary text of the widget, if any
        /// </summary>
        public string Text { get; set; }
            = "";

        /// <summary>
        /// Whether to draw the widget centered horizontally at the given position
        /// </summary>
        public bool Centered { get; set; }
            = false;

        /// <summary>
        /// Whether to draw a border. Only used by some widgets such as windows.
        /// </summary>
        public bool WithBorder { get; set; }
            = true;

        /// <summary>
        /// Bounds of the widget. Used by windows.
        /// </summary>
        public Rectangle Bounds { get; set; }
            = new Rectangle();

        /// <summary>
        /// Create a new widget drawing parameters object with given position.
        /// </summary>
        public WidgetDrawParams(Position position)
        {
            this.Position = position;
        }

        public WidgetDrawParams()
            : this(new Position())
        {

        }
    }
}
