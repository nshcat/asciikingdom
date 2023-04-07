using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Game.Ui;

namespace Game.Scenes.Common
{
    public class TestSubTabPage : SubTabPage
    {
        public TestSubTabPage(InputManager input, string title) : base(input, title)
        {
        }

        public override void Reshape(Size pageArea)
        {
            
        }

        public override void Render(Surface surface)
        {
            surface.DrawString(new Position(0, 0), $"Sub tab contents: {this.Title}", UiColors.ActiveText, DefaultColors.Black);
        }

        public override void Update(double deltaTime)
        {
            
        }
    }
}