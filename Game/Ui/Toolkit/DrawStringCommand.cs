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

        public DrawStringCommand(string str)
        {
            this._stringToDraw = str;
        }

        public void ApplyTo(RenderingContext context)
        {
            var (front, back) = context.GetRenderingColors();

            context.Target.DrawString(
                context.CurrentPosition,
                this._stringToDraw,
                front,
                back
            );
        }
    }
}
