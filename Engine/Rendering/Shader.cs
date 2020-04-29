using System;
using System.IO;
using OpenToolkit.Graphics.OpenGL4;

namespace Engine.Rendering
{
    /// <summary>
    /// An abstraction around OpenGL shader objects
    /// </summary>
    public class Shader
    {
        /// <summary>
        /// The native OpenGL handle to this object
        /// </summary>
        public int Handle { get; private set; }
        
        /// <summary>
        /// The type of this sahder
        /// </summary>
        public ShaderType Type { get; private set; }
        
        /// <summary>
        /// A piece of text detailing possible warnings etc resulting from compilation
        /// </summary>
        public string Log { get; private set; }
        
        /// <summary>
        /// Construct a new shader object in usable, but empty state.
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="ShaderException"></exception>
        private Shader(ShaderType type)
        {
            Type = type;
            
            // Request new shader object from OpenGL
            Handle = GL.CreateShader(type);

            if (Handle == 0)
            {
                throw new ShaderException("Failed to create new shader object");
            }
         
            // The shader object is now in created, but empty state.      
        }
        
        /// <summary>
        /// Create shader object from given source code.
        /// </summary>
        /// <param name="type">Type of the shader</param>
        /// <param name="content">Shader source code</param>
        /// <returns>Created shader object</returns>
        public static Shader FromText(ShaderType type, string content)
        {
            // Create shader and set source
            var shader = new Shader(type);
            GL.ShaderSource(shader.Handle, content);
            
            // Try to compiler shader
            GL.CompileShader(shader.Handle);  
    
            // Update shader stored shader log for later use
            shader.Log = GL.GetShaderInfoLog(shader.Handle);
            
            // Check for success
            GL.GetShader(shader.Handle, ShaderParameter.CompileStatus, out var result);

            if (result != 1)
            {
                throw new ShaderException("Failed to compile shader", shader.Log);
            }

            // Everything went okay, return shader object
            return shader;
        }
        
        /// <summary>
        /// Create shader object from text file contents.
        /// </summary>
        /// <param name="type">Type of the shader</param>
        /// <param name="path">Path to text file containing shader source code</param>
        /// <returns>Created shader object</returns>
        public static Shader FromFile(ShaderType type, string path)
        {
            return Shader.FromText(type, File.ReadAllText(path));
        }
    }
}