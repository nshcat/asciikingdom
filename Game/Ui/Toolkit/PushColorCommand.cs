using Engine.Core;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Ui.Toolkit
{
    /// <summary>
    /// Command that changes the either the front or back color
    /// </summary>
    public class PushColorCommand
        : IRenderingCommand
    {
        /// <summary>
        /// The new color after this command has executed.
        /// </summary>
        protected Color _newColor;

        /// <summary>
        /// The color slot that is to be affected
        /// </summary>
        protected ColorSlot _slot;

        public PushColorCommand(ColorSlot slot, Color color)
        {
            this._newColor = color;
            this._slot = slot;
        }

        public void ApplyTo(RenderingContext context)
        {
            context.PushColor(this._slot, this._newColor);
        }
    }
}
