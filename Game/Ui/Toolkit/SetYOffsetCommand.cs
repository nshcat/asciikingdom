using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core;

namespace Game.Ui.Toolkit
{
    /// <summary>
    /// Rendering command that sets a y offset
    /// </summary>
    public class SetYOffsetCommand
        : IRenderingCommand
    {
        /// <summary>
        /// The Y off set applied to all following rendering commands.
        /// </summary>
        public int Offset { get; set; }
            = 0;

        public SetYOffsetCommand(int offset)
        {
            this.Offset = offset;
        }

        public void ApplyTo(RenderingContext context)
        {
            context.Offset = new Position(0, this.Offset);
        }
    }
}
