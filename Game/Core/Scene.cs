using Engine.Core;
using Engine.Input;
using Engine.Rendering;
using Engine.Resources;

namespace Game.Core
{
    /// <summary>
    /// Abstract base class for all game scenes. Contains references to important engine components, and can be
    /// managed by a <see cref="SceneStack"/>
    /// </summary>
    public abstract class Scene : IRenderable, ILogic
    {
        /// <summary>
        /// Convenience reference to the scene stack.
        /// </summary>
        protected SceneStack SceneStack { get; set; }
        
        /// <summary>
        /// Convenience reference to the input manager.
        /// </summary>
        protected InputManager Input { get; set; }
        
        /// <summary>
        /// Convenience reference to the resource manager.
        /// </summary>
        protected ResourceManager Resources { get; set; }
        
        /// <summary>
        /// The dimensions of the screen, in pixels.
        /// </summary>
        public Size ScreenDimensions { get; set; }

        /// <summary>
        /// Create new scene, populating the engine component references from given parent scene.
        /// </summary>
        /// <param name="parent">Parent scene to retrieve engine component references from</param>
        public Scene(Scene parent)
        {
            this.SceneStack = parent.SceneStack;
            this.Input = parent.Input;
            this.Resources = parent.Resources;
        }

        /// <summary>
        /// Create new scene with given engine component references
        /// </summary>
        public Scene(SceneStack sceneStack, InputManager inputManager, ResourceManager resourceManager)
        {
            this.SceneStack = sceneStack;
            this.Input = inputManager;
            this.Resources = resourceManager;
        }
        
        public abstract void Render(RenderParams rp);
        public abstract void Update(double deltaTime);

        public virtual void Reshape(Size newSize)
        {
            this.ScreenDimensions = newSize;
        }

        /// <summary>
        /// Called everytime the scene gains focus
        /// </summary>
        public virtual void Activate()
        {
            
        }
    }
}