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
    /// Clas encapsulating how widgets are drawn
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// The color used to draw active text
        /// </summary>
        protected virtual Color ActiveTextColor => UiColors.ActiveText;

        /// <summary>
        /// The color used to draw inactive text
        /// </summary>
        protected virtual Color InactiveTextColor => UiColors.InactiveText;

        /// <summary>
        /// The main background color.
        /// </summary>
        protected virtual Color BackgroundColor => DefaultColors.Black;

        /// <summary>
        /// Front color for borders
        /// </summary>
        protected virtual Color BorderFrontColor => UiColors.BorderFront;

        /// <summary>
        /// Back color for borders
        /// </summary>
        protected virtual Color BorderBackColor => UiColors.BorderBack;

        /// <summary>
        /// Front color for window title text
        /// </summary>
        protected virtual Color WindowTitleColor => UiColors.BorderTitle;

        /// <summary>
        /// Draw a label
        /// </summary>
        public virtual Position DrawLabel(RenderCommandRecorder recorder, Position position, string label)
        {
            recorder.RecordPushFrontColor(this.ActiveTextColor);

            recorder.RecordDrawString(position, label);

            recorder.RecordPopFrontColor();

            return new Position(position.X + label.Length, position.Y);
        }

        /// <summary>
        /// Draw a button
        /// </summary>
        public virtual Position DrawButton(RenderCommandRecorder recorder, Position position, string label, bool isSelected, bool isEnabled)
        {
            recorder.RecordPushFrontColor(isEnabled ? this.ActiveTextColor : this.InactiveTextColor);

            var drawSelection = isSelected && isEnabled;

            if (drawSelection)
                recorder.RecordSetInverted();

            recorder.RecordDrawString(position, label);

            if (drawSelection)
                recorder.RecordSetNonInverted();

            recorder.RecordPopFrontColor();

            return new Position(position.X + label.Length, position.Y);
        }

        /// <summary>
        /// Draw a window
        /// </summary>
        /// <param name="recorder"></param>
        /// <param name="bounds"></param>
        /// <param name="title"></param>
        public virtual void DrawWindow(RenderCommandRecorder recorder, Rectangle bounds, string title, bool withBorder)
        {
            // The draw window command fills the background with the currently active back color.
            recorder.RecordPushBackColor(this.BackgroundColor);

            recorder.RecordDrawWindow(bounds, title, this.WindowTitleColor, this.BorderFrontColor, this.BorderBackColor, withBorder);

            recorder.RecordPopBackColor();
        }
    }
}
