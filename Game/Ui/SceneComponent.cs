using Engine.Core;
using Engine.Graphics;

namespace Game.Ui
{
    /// <summary>
    /// A component that can be drawn as part of a scene
    /// </summary>
    public abstract class SceneComponent
    {
        /// <summary>
        /// The position of the component in the scene.
        /// </summary>
        public Position Position { get; set; }
        
        /// <summary>
        /// The dimensions of the component, in tiles.
        /// </summary>
        public Size Dimensions { get; set; }
        
        /// <summary>
        /// Render component to given surface.
        /// </summary>
        public abstract void Render(Surface surface);
        
        
        public SceneComponent(Position position, Size dimensions)
        {
            this.Position = position;
            this.Dimensions = dimensions;
        }
    }
}