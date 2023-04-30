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
    /// The two color slots available in the context
    /// </summary>
    public enum ColorSlot
    {
        Front,
        Back
    }

    /// <summary>
    /// Context for executing rendering commands
    /// </summary>
    public class RenderingContext
    {
        /// <summary>
        /// Surface to draw to
        /// </summary>
        public Surface Target { get; protected set; }

        /// <summary>
        /// The current position
        /// </summary>
        public Position CurrentPosition { get; set; }
            = new Position(0, 0);

        /// <summary>
        /// Positional offset applied to all rendering commands
        /// </summary>
        public Position Offset { get; set; }
            = new Position(0, 0);

        /// <summary>
        /// Stack for front color changes
        /// </summary>
        protected Stack<Color> _frontColorStack
            = new Stack<Color>();

        /// <summary>
        /// Stack for back color changes
        /// </summary>
        protected Stack<Color> _backColorStack
            = new Stack<Color>();

        /// <summary>
        /// Whether the colors are drawn inverted
        /// </summary>
        public bool AreColorsInverted { get; set; }
            = false;

        /// <summary>
        /// Create a new rendering context for executing rendering commands on the given surface.
        /// </summary>
        public RenderingContext(Surface target)
        {
            this.Target = target;
            this._frontColorStack.Push(DefaultColors.White);
            this._backColorStack.Push(DefaultColors.Black); ;
        }

        /// <summary>
        /// Retrieve current rendering colors, taking into account inverted state.
        /// </summary>
        /// <returns>A tuple of colors (front, back)</returns>
        public (Color, Color) GetRenderingColors()
        {
            if (this.AreColorsInverted)
                return (this._backColorStack.Peek(), this._frontColorStack.Peek());
            else
                return (this._frontColorStack.Peek(), this._backColorStack.Peek());
        }

        /// <summary>
        /// Get the current drawing position
        /// </summary>
        public Position GetDrawingPosition()
        {
            return this.CurrentPosition + this.Offset;
        }

        /// <summary>
        /// Push a new color, making it active for the given slot
        /// </summary>
        public void PushColor(ColorSlot slot, Color color)
        {
            if(slot == ColorSlot.Front)
            {
                this._frontColorStack.Push(color);
            }
            else
            {
                this._backColorStack.Push(color);
            }
        }

        /// <summary>
        /// Pop the currently active color of the given slot, making the previous
        /// color the active one.
        /// </summary>
        public void PopColor(ColorSlot slot)
        {
            if (slot == ColorSlot.Front)
            {
                this._frontColorStack.Pop();
            }
            else
            {
                this._backColorStack.Pop();
            }
        }
    }
}
