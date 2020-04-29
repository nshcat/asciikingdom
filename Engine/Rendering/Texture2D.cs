using System;
using System.Runtime.InteropServices;
using OpenToolkit.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Engine.Rendering
{
    /// <summary>
    /// Represents a two-dimensional OpenGL texture object.
    /// </summary>
    public class Texture2D : Texture
    {
        /// <summary>
        /// Create new 2D texture from given source image.
        /// </summary>
        /// <param name="source"></param>
        public Texture2D(Image<Rgba32> source)
        {
            this.Handle = GL.GenTexture();
            if(this.Handle == 0)
                throw new OpenGlException("Failed to create texture object");
            
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, this.Handle);
            
            source.Mutate(new FlipProcessor(FlipMode.Vertical));
            
            var pixelBytes = MemoryMarshal.AsBytes(source.GetPixelSpan()).ToArray();

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, source.Width, source.Height,
                0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelBytes);
            
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        }

        /// <summary>
        /// Destroy the OpenGL objects associated with this instance.
        /// </summary>
        public void Destroy()
        {
            GL.DeleteTexture(this.Handle);
        }
        
        /// <summary>
        /// Activate and bind this texture to given texture unit for use in rendering.
        /// </summary>
        /// <param name="unit">Texture unit to bind texture to</param>
        public override void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, this.Handle);
        }
    }
}