using System;
using System.Runtime.Serialization;

namespace Engine.Rendering
{
    /// <summary>
    /// A special exception type used to signal errors in OpenGL code.
    /// </summary>
    public class OpenGlException : Exception
    {
        /// <summary>
        /// A text containing detailed information about the shader compilation
        /// or link failure.
        /// </summary>
        public string ErrorLog { get; private set; } = string.Empty;
        
        public OpenGlException()
        {
        }

        public OpenGlException(string message, string log) : base(message)
        {
            ErrorLog = log;
        }

        protected OpenGlException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public OpenGlException(string message) : base(message)
        {
        }

        public OpenGlException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}