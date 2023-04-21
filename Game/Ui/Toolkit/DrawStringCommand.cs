using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Ui.Toolkit
{
    /// <summary>
    /// Command that draw a fixed string
    /// </summary>
    public class DrawStringCommand
        : IRenderingCommand
    {
        /// <summary>
        /// The string that will be drawn.
        /// </summary>
        private string _stringToDraw;

        /// <summary>
        /// Whether the string will be drawn centered at the current position
        /// </summary>
        private bool _centered;

        public DrawStringCommand(string str, bool centered = false)
        {
            this._stringToDraw = str;
            this._centered = centered;
        }

        public void ApplyTo(RenderingContext context)
        {
            var (front, back) = context.GetRenderingColors();

            if (this._centered)
            {
                context.Target.DrawStringCentered(
                    context.CurrentPosition,
                    this._stringToDraw,
                    front,
                    back
                );
            }
            else
            {
                context.Target.DrawString(
                    context.CurrentPosition,
                    this._stringToDraw,
                    front,
                    back
                );
            }
        }
    }
}
