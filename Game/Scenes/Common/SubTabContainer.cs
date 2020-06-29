using System;
using System.Collections.Generic;
using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Game.Ui;
using OpenToolkit.Windowing.Common.Input;

namespace Game.Scenes.Common
{
    /// <summary>
    /// A tab page that contains a number of <see cref="SubTabPage"/> instances
    /// </summary>
    public class SubTabContainer : TabPage
    {
        /// <summary>
        /// All sub pages managed by this sub tab container
        /// </summary>
        protected List<SubTabPage> SubPages { get; set; }
            = new List<SubTabPage>();

        /// <summary>
        /// The index of the currently active sub tab page
        /// </summary>
        protected int CurrentIndex { get; set; }
            = 0;

        /// <summary>
        /// The currently active sub tab page
        /// </summary>
        protected SubTabPage CurrentPage => this.SubPages[this.CurrentIndex];

        /// <summary>
        /// Whether this sub tab container contains any sub tab pages
        /// </summary>
        protected bool HasPages => this.SubPages.Count > 0;

        /// <summary>
        /// Construct new sub tab container
        /// </summary>
        public SubTabContainer(InputManager input, string title, string header, int markedIndex, Key[] keyCombination)
            : base(input, title, header, markedIndex, keyCombination)
        {
        }

        /// <summary>
        /// React to tab client area size change
        /// </summary>
        public override void Reshape(Size pageArea)
        {
            foreach (var subPage in this.SubPages)
            {
                subPage.Reshape(pageArea);
            }
        }

        /// <summary>
        /// Render currently active sub tab page
        /// </summary>
        public override void Render(Surface surface)
        {
            if(this.HasPages)
                this.CurrentPage.Render(surface);
        }

        /// <summary>
        /// Render over parent container frame
        /// </summary>
        public override void RenderOverlay(Surface parentSurface)
        {
            base.RenderOverlay(parentSurface);
            this.DrawHeader(parentSurface);
        }

        /// <summary>
        /// Update sub tab page state
        /// </summary>
        public override void Update(double deltaTime)
        {
            if (this.Input.AreKeysDown(KeyPressType.Down, Key.Period))
                this.ChangeCurrentPage(1);
            
            if (this.Input.AreKeysDown(KeyPressType.Down, Key.Comma))
                this.ChangeCurrentPage(-1);
            
            if(this.HasPages)
                this.CurrentPage.Update(deltaTime);
        }

        /// <summary>
        /// Switch the currently active page
        /// </summary>
        /// <param name="direction">Switch direction</param>
        protected void ChangeCurrentPage(int direction)
        {
            this.CurrentIndex = Math.Max(0, Math.Min(this.SubPages.Count - 1, this.CurrentIndex + direction));
            
            if(this.HasPages)
                this.CurrentPage.Activate();
        }

        
        /// <summary>
        /// Draw the header containing the sub tab page titles
        /// </summary>
        protected void DrawHeader(Surface parentSurface)
        {
            var position = new Position(2, 8);

            for (var ix = 0; ix < this.SubPages.Count; ++ix)
            {
                var title = $"[{this.SubPages[ix].Title}]";
                var color = (ix == this.CurrentIndex) ? UiColors.ActiveText : UiColors.InactiveText;
                parentSurface.DrawString(
                    position,
                    title,
                    color,
                    DefaultColors.Black
                );
                
                position += new Position(title.Length + 1, 0);
            }
        }
    }
}