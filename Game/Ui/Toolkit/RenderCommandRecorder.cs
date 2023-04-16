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
    /// Base class enabling recording of render actions
    /// </summary>
    public abstract class RenderCommandRecorder
    {
        #region Abstract Interface
        protected abstract void Record(IRenderingCommand command);
        #endregion

        #region Helper Methods
        /// <summary>
        /// Record a draw string operation
        /// </summary>
        public void RecordDrawString(string str)
        {
            this.Record(new DrawStringCommand(str));
        }

        /// <summary>
        /// Record a set position operation
        /// </summary>
        public void RecordSetPosition(Position pos)
        {
            this.Record(new SetPositionCommand(pos));
        }

        /// <summary>
        /// Record a draw string at given location operation
        /// </summary>
        public void RecordDrawString(Position pos, string str)
        {
            this.RecordSetPosition(pos);
            this.RecordDrawString(str);
        }

        /// <summary>
        /// Record a push color action
        /// </summary>
        public void RecordPushColor(ColorSlot slot, Color color)
        {
            this.Record(new PushColorCommand(slot, color));
        }

        /// <summary>
        /// Record a front color change
        /// </summary>
        public void RecordPushFrontColor(Color color)
        {
            this.RecordPushColor(ColorSlot.Front, color);
        }

        /// <summary>
        /// Record a back color change
        /// </summary>
        public void RecordPushBackColor(Color color)
        {
            this.RecordPushColor(ColorSlot.Back, color);
        }

        /// <summary>
        /// Record a pop color action
        /// </summary>
        public void RecordPopColor(ColorSlot slot)
        {
            this.Record(new PopColorCommand(slot));
        }

        /// <summary>
        /// Record a pop front color action
        /// </summary>
        public void RecordPopFrontColor()
        {
            this.RecordPopColor(ColorSlot.Front);
        }

        /// <summary>
        /// Record a pop back color action
        /// </summary>
        public void RecordPopBackColor()
        {
            this.RecordPopColor(ColorSlot.Back);
        }

        /// <summary>
        /// Record inverted state
        /// </summary>
        public void RecordSetInvertedState(bool isInverted)
        {
            this.Record(new SetInvertedCommand(isInverted));
        }

        /// <summary>
        /// Record inverted state being on
        /// </summary>
        public void RecordSetInverted()
        {
            this.RecordSetInvertedState(true);
        }

        /// <summary>
        /// Record inverted state being off
        /// </summary>
        public void RecordSetNonInverted()
        {
            this.RecordSetInvertedState(false);
        }
        #endregion
    }
}
