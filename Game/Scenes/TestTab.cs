using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Game.Ui;
using OpenToolkit.Windowing.Common.Input;

namespace Game.Scenes
{
    public class TestTab : SubTabContainer
    {
        public TestTab(InputManager input, string title, string header, int markedIndex, params Key[] keyCombination)
            : base(input, title, header, markedIndex, keyCombination)
        {
            this.SubPages.Add(new TestSubTabPage(input, "Summary"));
            this.SubPages.Add(new TestSubTabPage(input, "Needs"));
            this.SubPages.Add(new TestSubTabPage(input, "Growth"));
        }
    }
}