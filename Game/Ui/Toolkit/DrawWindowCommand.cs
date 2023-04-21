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

        /// <summary>
        /// Foreground color for border
        /// </summary>
        protected Color _borderFrontColor;

        /// <summary>
        /// Background color for border
        /// </summary>
        protected Color _borderBackColor;

        /// <summary>
        /// Whether the window has a border
        /// </summary>
        protected bool _drawBorder;

        public DrawWindowCommand(Rectangle bounds, string title,
            Color titleFrontColor, Color borderFrontColor, Color borderBackColor,
            bool drawBorder)
        {
            this._bounds = bounds;
            this._title = title;
            this._titleFrontColor = titleFrontColor;
            this._borderBackColor = borderBackColor;
            this._borderFrontColor = borderFrontColor;
            this._drawBorder = drawBorder;
        }

        public void ApplyTo(RenderingContext context)
        {
            var (_, back) = context.GetRenderingColors();

            if (this._drawBorder)
            {
                context.Target.DrawWindow(this._bounds, this._title,
                    this._borderFrontColor, this._borderBackColor, this._titleFrontColor, back);
            }
            else
            {
                context.Target.FillArea(this._bounds, back);
            }
        }
    }
}
