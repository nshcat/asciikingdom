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
        public void RecordDrawString(string str, bool centered = false)
        {
            this.Record(new DrawStringCommand(str, centered));
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
        public void RecordDrawString(Position pos, string str, bool centered = false)
        {
            this.RecordSetPosition(pos);
            this.RecordDrawString(str, centered);
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

        /// <summary>
        /// Record setting a clipping mask
        /// </summary>
        public void RecordSetClippingMask(Rectangle mask)
        {
            this.Record(new SetClippingMaskCommand(mask));
        }

        /// <summary>
        /// Record clearing a clipping mask
        /// </summary>
        public void RecordClearClippingMask()
        {
            this.Record(new SetClippingMaskCommand());
        }

        /// <summary>
        /// Record setting a y offset
        /// </summary>
        public SetYOffsetCommand RecordSetOffset(int y)
        {
            var command = new SetYOffsetCommand(y);
            this.Record(command);
            return command;
        }

        /// <summary>
        /// Record clearing a y offset
        /// </summary>
        public void RecordClearOffset()
        {
            this.Record(new SetYOffsetCommand(0));
        }

        /// <summary>
        /// Record the drawing of a tile
        /// </summary>
        public void RecordDrawTile(int glyph)
        {
            this.Record(new DrawTileCommand(glyph));
        }

        /// <summary>
        /// Record the drawing of a tile at given position
        /// </summary>
        public void RecordDrawTile(Position position, int glyph)
        {
            this.RecordSetPosition(position);
            this.RecordDrawTile(glyph);
        }

        /// <summary>
        /// Record drawing of a window
        /// </summary>
        public void RecordDrawWindow(Rectangle bounds, string title,
            Color titleFrontColor, Color borderFrontColor, Color borderBackColor, bool drawBorder)
        {
            this.Record(new DrawWindowCommand(bounds, title, titleFrontColor, borderFrontColor, borderBackColor, drawBorder));
        }
        #endregion
    }
}
