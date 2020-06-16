using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Game.Core;
using OpenToolkit.Windowing.Common.Input;

namespace Game.Ui
{
    /// <summary>
    /// A sub window used to ask the user to input some text
    /// </summary>
    public class TextInputWindow : SubWindow
    {
        /// <summary>
        /// The current text in the text box
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Maximum allowed text sized
        /// </summary>
        private static int TextMaxLength { get; } = 25;
        
        protected TextInputWindow(string title, Either<int, float> width, Either<int, float> height)
            : base(title, width, height)
        {
        }

        public TextInputWindow(string title, int width)
            : base(title, Either<int, float>.WithLeft(width), Either<int, float>.WithLeft(5))
        {
        }

        public TextInputWindow(string title, float width)
            : base(title, Either<int, float>.WithRight(width), Either<int, float>.WithLeft(5))
        {
        }

        protected override void RenderInner(Surface surface, Rectangle innerBounds)
        {
            surface.DrawString(
                    innerBounds.TopLeft + new Position(1, 1),
                    this.Text + "_",
                    UiColors.ActiveText,
                    DefaultColors.Black
                );
        }

        /// <summary>
        /// Erase any previously stored text, and make text input window ready for next use.
        /// </summary>
        public override void Begin()
        {
            base.Begin();
            this.Text = "";
        }

        /// <summary>
        /// Register any user text input
        /// </summary>
        public void Update(InputManager inputManager)
        {
            if(inputManager.Text != null
               && this.Text.Length + inputManager.Text.Length <= TextMaxLength)
                this.Text += inputManager.Text;

            if (inputManager.IsKeyDown(KeyPressType.Down, Key.BackSpace) && this.Text.Length > 0)
                this.Text = this.Text.Remove(this.Text.Length - 1);
        }
        
    }
}