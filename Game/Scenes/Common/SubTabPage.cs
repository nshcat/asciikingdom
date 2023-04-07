using Engine.Core;
using Engine.Graphics;
using Engine.Input;

namespace Game.Scenes.Common
{
    /// <summary>
    /// Base class for sub tab pages, which are managed and drawn by a <see cref="SubTabContainer"/>
    /// </summary>
    public abstract class SubTabPage
    {
        /// <summary>
        /// The sub tab title
        /// </summary>
        public string Title { get; protected set; }
        
        /// <summary>
        /// The input manager used to react to user input
        /// </summary>
        public InputManager Input { get; protected set; }

        /// <summary>
        /// Construct new sub tab page
        /// </summary>
        public SubTabPage(InputManager input, string title)
        {
            this.Input = input;
            this.Title = title;
        }
        
        /// <summary>
        /// React to client area size change.
        /// </summary>
        /// <param name="pageArea">The area that is owned by this sub tab page.</param>
        public abstract void Reshape(Size pageArea);

        /// <summary>
        /// Draw sub tab page client are to given surface.
        /// </summary>
        /// <param name="surface">Surface spanning the whole client area owned by this sub tab page.</param>
        public abstract void Render(Surface surface);

        /// <summary>
        /// Perform logic update based on given elapsed time.
        /// </summary>
        /// <param name="deltaTime">Number of seconds elapsed since last update.</param>
        /// <remarks>
        /// Sub tab pages are supposed to do their own input management using the <see cref="Input"/> attribute.
        /// </remarks>
        public abstract void Update(double deltaTime);

        /// <summary>
        /// Called when ever this sub tab page is selected by the user.
        /// </summary>
        public virtual void Activate()
        {
            
        }
    }
}