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
    public class PopColorCommand
        : IRenderingCommand
    {
        /// <summary>
        /// Which color slot is to be affected
        /// </summary>
        protected ColorSlot _slot;

        public PopColorCommand(ColorSlot slot)
        {
            this._slot = slot;
        }

        public void ApplyTo(RenderingContext context)
        {
            context.PopColor(this._slot);
        }
    }
}
