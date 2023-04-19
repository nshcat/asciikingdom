using Engine.Core;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Ui.Toolkit
{
    public class DrawWindowCommand
        : IRenderingCommand
    {
        /// <summary>
        /// The bounds of the border
        /// </summary>
        protected Rectangle _bounds;

        /// <summary>
        /// The window title
        /// </summary>
        protected string _title;

        /// <summary>
        /// Foreground text color for title
        /// </summary>
        protected Color _titleFrontColor;

        public DrawWindowCommand(Rectangle bounds, string title, Color titleFrontColor)
        {
            this._bounds = bounds;
            this._title = title;
            this._titleFrontColor = titleFrontColor;
        }

        public void ApplyTo(RenderingContext context)
        {
            var (front, back) = context.GetRenderingColors();

            context.Target.DrawWindow(this._bounds, this._title, front, back, this._titleFrontColor, back);
        }
    }
}
