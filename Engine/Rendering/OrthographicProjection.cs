using Engine.Core;
using OpenToolkit.Mathematics;

namespace Engine.Rendering
{
    /// <summary>
    /// Represents a simple orthographic projection with fixed view matrix for use with OpenGL.
    /// </summary>
    public class OrthographicProjection
    {
        /// <summary>
        /// The view matrix. This always is the identity matrix.
        /// </summary>
        public Matrix4 View => Matrix4.Identity;
        
        /// <summary>
        /// The projection matrix corresponding to this orthographic projection.
        /// </summary>
        public Matrix4 Projection { get; protected set; }
        
        /// <summary>
        /// Retrieve <see cref="RenderParams"/> instance based on this projection.
        /// </summary>
        public RenderParams Params => new RenderParams(this.View, this.Projection);

        /// <summary>
        /// Refresh this projection based on given screen dimensions
        /// </summary>
        /// <param name="screenDimensions">Screen dimensions, in pixels</param>
        public void Refresh(Size screenDimensions)
        {
            // We flip the y-axis, in order to have (0,0) at the top left of the screen
            this.Projection = Matrix4.CreateOrthographicOffCenter(0.0f, (float) screenDimensions.Width,
                (float) screenDimensions.Height, 0.0f, 0.0f, 1.0f);
        }
    }
}