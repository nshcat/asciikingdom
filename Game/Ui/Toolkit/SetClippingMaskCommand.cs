using Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Ui.Toolkit
{
    /// <summary>
    /// Rendering command that sets a clipping mask
    /// </summary>
    public class SetClippingMaskCommand
        : IRenderingCommand
    {
        /// <summary>
        /// The mask to set. Can be null.
        /// </summary>
        protected Rectangle? _mask;

        public SetClippingMaskCommand(Rectangle rect)
        {
            this._mask = rect;
        }

        public SetClippingMaskCommand()
        {
            this._mask = null;
        }

        public void ApplyTo(RenderingContext context)
        {
            context.Target.ClippingMask = this._mask;
        }
    }
}
