using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using OpenToolkit.Windowing.Common.Input;

namespace Game.Scenes
{
    /// <summary>
    /// A single tab page, meant to be contained within a <see cref="TabContainerScene"/>
    /// </summary>
    public abstract class TabPage
    {
        /// <summary>
        /// The current input manager.
        /// </summary>
        public InputManager Input { get; protected set; }
        
        /// <summary>
        /// The title of this tab page, shown in the tab bar of the <see cref="TabContainerScene"/>.
        /// </summary>
        public string Title { get; protected set; }
        
        /// <summary>
        /// The index of the marked hot key character in the <see cref="Title"/>
        /// </summary>
        public int MarkedIndex { get; protected set; }
        
        /// <summary>
        /// The key combination that acts as a hot key to select this tab page
        /// </summary>
        public Key[] KeyCombination { get; protected set; }

        /// <summary>
        /// Initialize new tab page and its attributes.
        /// </summary>
        public TabPage(string title, int markedIndex, Key[] keyCombination)
        {
            this.Title = title;
            this.MarkedIndex = markedIndex;
            this.KeyCombination = keyCombination;
        }
        
        /// <summary>
        /// React to client area size change.
        /// </summary>
        /// <param name="pageArea">The area that is owned by this tab page.</param>
        public abstract void Reshape(Size pageArea);

        /// <summary>
        /// Draw tab page client are to given surface.
        /// </summary>
        /// <param name="surface">Surface spanning the whole client area owned by this tab page.</param>
        public abstract void Render(Surface surface);

        /// <summary>
        /// Perform logic update based on given elapsed time.
        /// </summary>
        /// <param name="deltaTime">Number of seconds elapsed since last update.</param>
        /// <remarks>
        /// Tab pages are supposed to do their own input management using the <see cref="Input"/> attribute.
        /// </remarks>
        public abstract void Update(double deltaTime);

        /// <summary>
        /// Called when ever this tab page is selected by the user.
        /// </summary>
        public abstract void Activate();
    }
}