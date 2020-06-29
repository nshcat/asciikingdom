using System.Collections.Generic;
using System.Linq;
using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Engine.Rendering;
using Engine.Resources;
using Game.Core;
using Game.Ui;
using OpenToolkit.Windowing.Common.Input;

namespace Game.Scenes
{
    
    /// <summary>
    /// A scene that contains and manages a number of <see cref="TabPage"/> instances.
    /// </summary>
    public abstract class TabContainerScene : Scene
    {
        /// <summary>
        /// The title of the container frame
        /// </summary>
        protected abstract string Title { get; }
        
        /// <summary>
        /// All registered tab pages in this tab container
        /// </summary>
        protected List<TabPage> Pages { get; set; }
            = new List<TabPage>();

        /// <summary>
        /// The index of the currently selected tab
        /// </summary>
        protected int CurrentIndex { get; set; }
            = 0;

        /// <summary>
        /// The currently selected tab
        /// </summary>
        protected TabPage CurrentTab => this.Pages[this.CurrentIndex];

        /// <summary>
        /// Whether this tab container contains any pages
        /// </summary>
        protected bool HasPages => this.Pages.Count > 0;

        /// <summary>
        /// Surface spanning the whole screen, used to draw the container outline and tab bar
        /// </summary>
        protected Surface ContainerSurface { get; set; }
        
        /// <summary>
        /// The surface used to draw the tab page contents
        /// </summary>
        protected Surface PageSurface { get; set; }

        /// <summary>
        /// Construct new tab container scene
        /// </summary>
        protected TabContainerScene(Scene parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Construct new tab container scene
        /// </summary>
        protected TabContainerScene(SceneStack sceneStack, InputManager inputManager, ResourceManager resourceManager)
            : base(sceneStack, inputManager, resourceManager)
        {
        }

        /// <summary>
        /// Render scene to screen
        /// </summary>
        public override void Render(RenderParams rp)
        {
            this.ContainerSurface.Clear();
            this.PageSurface.Clear();
            
            this.DrawContainer();

            if (this.HasPages)
            {
                this.CurrentTab.Render(this.PageSurface);
                this.CurrentTab.RenderOverlay(this.ContainerSurface);
            }
            
            this.ContainerSurface.Render(rp);
            this.PageSurface.Render(rp);
        }

        /// <summary>
        /// Update scene state
        /// </summary>
        public override void Update(double deltaTime)
        {
            // Check if we are supposed to exit this scene
            if (this.Input.AreKeysDown(KeyPressType.Down, Key.Escape)
                || this.Input.AreKeysDown(KeyPressType.Down, Key.ShiftLeft, Key.Q))
            {
                this.SceneStack.NextOperation = new SceneStackOperation.PopScene();
                return;
            }
            
            // Sort the pages by the size of their key combination. This implements "greedy matching" of hot keys:
            // If there multiple pages with the same letter key but different amount of modifiers, the one
            // with the most modifiers is selected, which is what most users would expect
            var pages = this.Pages.OrderByDescending(p => p.KeyCombination.Length).ToList();

            for (var i = 0; i < pages.Count; ++i)
            {
                // Check if the key combination of the i-th tab page was activated by the user
                if (this.Input.AreKeysDown(KeyPressType.Down, pages[i].KeyCombination))
                {
                    this.CurrentIndex = i;
                    this.CurrentTab.Activate();
                    break;
                }
            }
            
            if(this.HasPages)
                this.CurrentTab.Update(deltaTime);
        }

        /// <summary>
        /// React to screen changes
        /// </summary>
        public override void Reshape(Size newSize)
        {
            base.Reshape(newSize);
            
            this.PageSurface?.Destroy();
            this.ContainerSurface?.Destroy();
            
            this.ContainerSurface = Surface.New()
                .Tileset(this.Resources, "myne_rect.png")
                .PixelDimensions(this.ScreenDimensions)
                .Build();

            var pageArea = this.ContainerSurface.Dimensions - new Size(2, 10);
            
            this.PageSurface = Surface.New()
                .Tileset(this.Resources, "myne_rect.png")
                .RelativeTo(this.ContainerSurface, new Position(1, 9))
                .TileDimensions(pageArea)
                .Build();

            foreach (var page in this.Pages)
            {
                page.Reshape(pageArea);
            }
        }

        /// <summary>
        /// Draw the tab container elements
        /// </summary>
        protected void DrawContainer()
        {
            this.DrawFrame();
            this.DrawTabs();
        }

        /// <summary>
        /// Draw the tab header
        /// </summary>
        protected void DrawTabs()
        {
            var position = new Position(2, 2);

            for(var i = 0; i < this.Pages.Count; ++i)
            {
                var page = this.Pages[i];
                var selected = this.CurrentIndex == i;
                var label = selected ? $">{page.Title}<" : $" {page.Title} ";

                this.ContainerSurface.DrawString(position, label, UiColors.ActiveText, DefaultColors.Black);

                var character = page.Title[page.MarkedIndex];
                this.ContainerSurface.SetTile(
                    position + new Position(page.MarkedIndex + 1, 0),
                    new Tile(
                        character,
                        UiColors.Keybinding,
                        DefaultColors.Black
                    )
                );
                
                position += new Position(label.Length + 1, 0);
            }

            if (this.HasPages)
            {
                this.ContainerSurface.DrawString(
                    new Position(2, 6),
                    this.CurrentTab.Header,
                    UiColors.ActiveText,
                    DefaultColors.Black
                );
                
                var tile = new Tile((int)BoxCharacters.Horizontal, UiColors.BorderFront, DefaultColors.Black);

                for (var ix = 1; ix < this.ContainerSurface.Dimensions.Width - 1; ++ix)
                {
                    this.ContainerSurface.SetTile(new Position(ix, 8), tile);
                }
                
                this.ContainerSurface.SetTile(
                    new Position(0, 8),
                    new Tile(
                        (int)BoxCharacters.VerticalRight,
                        UiColors.BorderFront,
                        UiColors.BorderBack
                    )
                );
                
                this.ContainerSurface.SetTile(
                    new Position(this.ContainerSurface.Dimensions.Width - 1, 8),
                    new Tile(
                        (int)BoxCharacters.VerticalLeft,
                        UiColors.BorderFront,
                        UiColors.BorderBack
                    )
                );
            }
        }

        /// <summary>
        /// Draw the container frame and title
        /// </summary>
        protected void DrawFrame()
        {
            this.ContainerSurface.DrawWindow(
                new Rectangle(this.ContainerSurface.Dimensions),
                this.Title,
                UiColors.BorderFront,
                UiColors.BorderBack,
                UiColors.BorderTitle,
                DefaultColors.Black
            );

            for (var ix = 1; ix < this.ContainerSurface.Dimensions.Width - 1; ++ix)
            {
                var position = new Position(ix, 4);
                this.ContainerSurface.SetTile(position,
                    new Tile(
                        (int)BoxCharacters.Horizontal,
                        UiColors.BorderFront,
                        UiColors.BorderBack
                    )
                );
            }
            
            this.ContainerSurface.SetTile(new Position(0, 4), 
                new Tile(
                    (int)BoxCharacters.VerticalRight,
                    UiColors.BorderFront,
                    UiColors.BorderBack
                )
            );
            
            this.ContainerSurface.SetTile(new Position(this.ContainerSurface.Dimensions.Width - 1, 4), 
                new Tile(
                    (int)BoxCharacters.VerticalLeft,
                    UiColors.BorderFront,
                    UiColors.BorderBack
                )
            );
        }
    }
}