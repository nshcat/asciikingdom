using System;

namespace Game.Data
{
    /// <summary>
    /// Represents an exceptional condition when handling game data
    /// </summary>
    public class DataException : Exception
    {
        public DataException()
        {
            
        }

        public DataException(string message)
            : base(message)
        {
            
        }

        public DataException(string message, Exception inner)
            : base(message, inner)
        {
            
        }
    }
}