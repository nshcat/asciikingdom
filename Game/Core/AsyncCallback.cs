using System;
using System.Threading;

namespace Game.Core
{
    /// <summary>
    /// Represents a callback delegate issued by a async operation.
    /// </summary>
    public class AsyncCallback
    {
        /// <summary>
        /// The delegate to execute
        /// </summary>
        public Action Callback { get; }
        
        /// <summary>
        /// Reset event used to signal finished callback execution to the async operation
        /// </summary>
        public ManualResetEvent WasExecuted { get; }

        /// <summary>
        /// Create a new async callback instance based on given action.
        /// </summary>
        public AsyncCallback(Action callback)
        {
            this.Callback = callback;
            this.WasExecuted = new ManualResetEvent(false);
        }
    }
}