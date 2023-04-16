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
    /// Command that enabled inverted color mode
    /// </summary>
    public class SetInvertedCommand
        : IRenderingCommand
    {
        /// <summary>
        /// Whether the colors will be inverted
        /// </summary>
        protected bool _isInverted = false;

        public SetInvertedCommand(bool isInverted)
        {
            this._isInverted = isInverted;
        }

        public void ApplyTo(RenderingContext context)
        {
            context.AreColorsInverted = this._isInverted;
        }
    }
}
