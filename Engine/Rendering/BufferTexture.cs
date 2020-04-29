using System;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;

namespace Engine.Rendering
{
    /// <summary>
    /// Wrapper for OpenGL buffer textures, which are one-dimensional textures backed by a GPU
    /// buffer object. This class only supports storing integers.
    /// </summary>
    public class BufferTexture: Texture
    {
        /// <summary>
        /// The handle of the buffer object backing this buffer texture.
        /// </summary>
        public int BufferHandle { get; private set; }
        
        /// <summary>
        /// Size of the buffer texture, in integers.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Create a new buffer texture backed by a buffer object of given size.
        /// </summary>
        /// <param name="size">Size of the backing buffer object</param>
        public BufferTexture(int size)
        {
            this.Resize(size);
        }

        /// <summary>
        /// Resize the buffer texture to be of given size.
        /// </summary>
        /// <param name="size">New buffer texture size</param>
        public void Resize(int size)
        {
            // Negative buffer sizes are obviously not allowed
            if(size < 0)
                throw new ArgumentException("Negative buffer size not allowed");

            this.Size = size;
            
            // Delete old buffer texture if needed
            if (this.BufferHandle != 0)
            {
                GL.DeleteTexture(this.Handle);
                GL.DeleteBuffer(this.BufferHandle);
            }
            
            // Create texture object
            GL.ActiveTexture(TextureUnit.Texture0);
            this.Handle = GL.GenTexture();
            if(this.Handle == 0)
                throw new OpenGlException("Failed to create BufferTexture texture object");
            
            // Create buffer object
            this.BufferHandle = GL.GenBuffer();
            if(this.BufferHandle == 0)
                throw new OpenGlException("Failed to create BufferTexture backing buffer object");
            
            // Bind buffer and fill with zeros
            GL.BindBuffer(BufferTarget.TextureBuffer, this.BufferHandle);
            GL.BufferData(BufferTarget.TextureBuffer, (this.Size * 4), IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindTexture(TextureTarget.TextureBuffer, this.Handle);
            GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.Rgba32ui, this.BufferHandle);
        }

        /// <summary>
        /// Activate and bind this buffer texture to given texture unit.
        /// </summary>
        /// <param name="unit">Texture unit to bind to</param>
        public override void Use(TextureUnit unit)
        {
            GL.BindBuffer(BufferTarget.TextureBuffer, this.BufferHandle);
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.TextureBuffer, this.Handle);
        }

        /// <summary>
        /// Upload given integer array to the buffer texture. It is expected to be the exact same size
        /// as the buffer.
        /// </summary>
        /// <param name="buffer">Integer array containing data to upload</param>
        public void Upload(int[] buffer)
        {
            if (buffer.Length != this.Size)
                throw new ArgumentException("Source buffer size has to match buffer texture size");
            
            GL.BindBuffer(BufferTarget.TextureBuffer, this.BufferHandle);
            GL.BufferSubData(BufferTarget.TextureBuffer, IntPtr.Zero, this.Size * 4, buffer);
        }
        
        /// <summary>
        /// Destroy the OpenGL texture buffer object associated with this instance.
        /// </summary>
        public void Destroy()
        {
            if (this.Handle != 0)
            {
                GL.DeleteTexture(this.Handle);
                GL.DeleteBuffer(this.BufferHandle);

                this.Handle = 0;
            }
        }
    }
}