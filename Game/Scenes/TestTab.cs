using Engine.Core;
using Engine.Graphics;
using Game.Ui;
using OpenToolkit.Windowing.Common.Input;

namespace Game.Scenes
{
    public class TestTab : TabPage
    {
        public TestTab(string title, int markedIndex, params Key[] keyCombination) : base(title, markedIndex, keyCombination)
        {
        }

        public override void Reshape(Size pageArea)
        {
            
        }

        public override void Render(Surface surface)
        {
            surface.DrawString(new Position(0, 0), "Meow", UiColors.ActiveText, DefaultColors.Black);
        }

        public override void Update(double deltaTime)
        {
            
        }

        public override void Activate()
        {
            throw new System.NotImplementedException();
        }
    }
}