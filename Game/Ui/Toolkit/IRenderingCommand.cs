using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Ui.Toolkit
{
    /// <summary>
    /// Interface for UI rendering commands
    /// </summary>
    public interface IRenderingCommand
    {
        /// <summary>
        /// Apply rendering command to given context.
        /// </summary>
        public abstract void ApplyTo(RenderingContext context);
    }
}
