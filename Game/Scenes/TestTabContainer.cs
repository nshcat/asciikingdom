using Engine.Input;
using Engine.Resources;
using Game.Core;
using OpenToolkit.Windowing.Common.Input;

namespace Game.Scenes
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
            this.Pages.Add(new TestTab("Overview", 0, Key.O, Key.ShiftLeft));
            this.Pages.Add(new TestTab("Production", 0, Key.P, Key.ShiftLeft));
        }

        protected override string Title { get; } = "Meow";
    }
}