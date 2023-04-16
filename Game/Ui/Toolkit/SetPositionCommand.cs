using Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Ui.Toolkit
{
    /// <summary>
    /// Command setting the current rendering position
    /// </summary>
    public class SetPositionCommand
        : IRenderingCommand
    {
        /// <summary>
        /// The new position after this command has executed.
        /// </summary>
        protected Position _newPosition;

        public SetPositionCommand(Position pos)
        {
            this._newPosition = pos;
        }

        public void ApplyTo(RenderingContext context)
        {
            context.CurrentPosition = this._newPosition;
        }
    }
}
