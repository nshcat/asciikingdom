using OpenToolkit.Graphics.OpenGL4;

namespace Engine.Rendering
{
    /// <summary>
    /// Class wrapping empty VAO and VBO, needed on some GPUs to support the no-geometry rendering
    /// the ASCII surfaces are doing.
    /// </summary>
    public class EmptyBuffers
    {
        /// <summary>
        /// The VAO handle
        /// </summary>
        public int VAOHandle { get; protected set; }
        
        /// <summary>
        /// The VBO handle
        /// </summary>
        public int VBOHandle { get; protected set; }
        
        /// <summary>
        /// Create new instance
        /// </summary>
        public EmptyBuffers()
        {
            this.VBOHandle = GL.GenBuffer();
            if(this.VBOHandle == 0)
                throw new OpenGlException("Failed to create empty VBO");

            this.VAOHandle = GL.GenVertexArray();
            if(this.VAOHandle == 0)
                throw new OpenGlException("Failed to create empty VAO");
        }

        /// <summary>
        /// Activate and bind both the VAO and VBO
        /// </summary>
        public void Use()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.VBOHandle);
            GL.BindVertexArray(this.VAOHandle);
        }
    }
}