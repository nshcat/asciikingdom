using System;
using System.Runtime.Serialization;

namespace Engine.Rendering
{
    /// <summary>
    /// A special exception type used to signal errors in shader and shader program compilation or linking.
    /// </summary>
    public class ShaderException : Exception
    {
        /// <summary>
        /// A text containing detailed information about the shader compilation
        /// or link failure.
        /// </summary>
        public string ErrorLog { get; private set; } = string.Empty;
        
        public ShaderException()
        {
        }

        public ShaderException(string message, string log) : base(message)
        {
            ErrorLog = log;
        }

        protected ShaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShaderException(string message) : base(message)
        {
        }

        public ShaderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}