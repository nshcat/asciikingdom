using System.Reflection.Emit;
using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Game.Core;
using Game.Simulation;

namespace Game.Ui
{
    /// <summary>
    /// A centered sub window meant to be displayed over other content
    /// </summary>
    public abstract class SubWindow
    {
        /// <summary>
        /// Width of this sub window, provided either as an absolute value or as an percentage of the parent
        /// surface width
        /// </summary>
        public Either<int, float> Width { get; }
        
        /// <summary>
        /// Width of this sub window, provided either as an absolute value or as an percentage of the parent
        /// surface width
        /// </summary>
        public Either<int, float> Height { get; }
        
        /// <summary>
        /// The title of the sub window
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The calculated outer bounds of the sub window
        /// </summary>
        private Rectangle _bounds;

        /// <summary>
        /// The calculated bounds of the inner panel
        /// </summary>
        private Rectangle _innerBounds;

        /// <summary>
        /// Create new sub window instance, with given width and height values
        /// </summary>
        public SubWindow(string title, Either<int, float> width, Either<int, float> height)
        {
            this.Title = title;
            this.Width = width;
            this.Height = height;
        }
        
        /// <summary>
        /// Create new sub window instance, with given absolute width and height
        /// </summary>
        public SubWindow(string title, int width, int height)
            : this(title, Either<int, float>.WithLeft(width), Either<int, float>.WithLeft(height))
        {
            
        }
        
        /// <summary>
        /// Create new sub window instance, with given relative width and absolute height
        /// </summary>
        public SubWindow(string title, float width, int height)
            : this(title, Either<int, float>.WithRight(width), Either<int, float>.WithLeft(height))
        {
            
        }
        
        /// <summary>
        /// Create new sub window instance, with given absolute width and relative height
        /// </summary>
        public SubWindow(string title, int width, float height)
            : this(title, Either<int, float>.WithLeft(width), Either<int, float>.WithRight(height))
        {
            
        }
        
        /// <summary>
        /// Create new sub window instance, with given relative width and height
        /// </summary>
        public SubWindow(string title, float width, float height)
            : this(title, Either<int, float>.WithRight(width), Either<int, float>.WithRight(height))
        {
            
        }

        /// <summary>
        /// Render the inner content of the sub window
        /// </summary>
        protected abstract void RenderInner(Surface surface, Rectangle innerBounds);

        /// <summary>
        /// Method that can be used to implement logic that needs to be run every time the
        /// sub window is going to be reused
        /// </summary>
        public virtual void Begin()
        {
            
        }

        /// <summary>
        /// Method that can be used to implement logic that needs to be run every time the sub window is done
        /// with being used
        /// </summary>
        public virtual void End()
        {
            
        }

        /// <summary>
        /// React to screen dimension changes
        /// </summary>
        public virtual void Reshape(Surface surface)
        {
            var width = this.Width.Unpack(
                x => x,
                x => (int)(surface.Dimensions.Width * x)
            );
            
            var height = this.Height.Unpack(
                x => x,
                x => (int)(surface.Dimensions.Height * x)
            );

            this._bounds = new Rectangle(surface.Dimensions).Centered(new Size(width, height));
            this._innerBounds = new Rectangle(surface.Dimensions).Centered(new Size(width-2, height-2));
        }

        /// <summary>
        /// Render the sub window to given surface
        /// </summary>
        public void Render(Surface surface)
        {
            surface.DrawWindow(
                this._bounds,
                this.Title,
                UiColors.BorderFront,
                UiColors.BorderBack,
                UiColors.BorderTitle,
                DefaultColors.Black
            );
            
            this.RenderInner(surface, this._innerBounds);
        }
    }
}