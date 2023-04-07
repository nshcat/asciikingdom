using Engine.Input;
using Engine.Resources;
using Game.Core;
using OpenToolkit.Windowing.Common.Input;

namespace Game.Scenes.Common
{
    public class TestTabContainer : TabContainerScene
    {
        public TestTabContainer(Scene parent) : base(parent)
        {
            this.AddPages();
        }

        public TestTabContainer(SceneStack sceneStack, InputManager inputManager, ResourceManager resourceManager) : base(sceneStack, inputManager, resourceManager)
        {
            this.AddPages();
        }

        protected void AddPages()
        {
            this.Pages.Add(new TestTab(this.Input, "Overview", "Site overview", 0, Key.O, Key.ShiftLeft));
            this.Pages.Add(new TestTab(this.Input, "Construction","Construction queue", 0, Key.C, Key.ShiftLeft));
            this.Pages.Add(new TestTab(this.Input, "Production","Production overview", 0, Key.P, Key.ShiftLeft));
            this.Pages.Add(new TestTab(this.Input, "Policies","City policies", 1, Key.O));
        }

        protected override string Title { get; } = "Meow";
    }
}