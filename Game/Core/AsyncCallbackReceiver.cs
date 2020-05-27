using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// A callback sink used with <see cref="AsyncOperation"/>. It receives callbacks from the async
    /// operation in the form of delegates, that can be executed by the main game thread.
    /// </summary>
    public class AsyncCallbackReceiver
    {
        /// <summary>
        /// The queue holding all received callbacks.
        /// </summary>
        protected Queue<AsyncCallback> CallbackQueue { get; } = new Queue<AsyncCallback>();

        /// <summary>
        /// Register given callback to be executed on the main game thread.
        /// </summary>
        /// <remarks>
        /// This method is normally called by an async operation wishing to perform operations
        /// in the main game thread.
        /// </remarks>
        public void BeginInvoke(AsyncCallback callback)
        {
            lock (this.CallbackQueue)
            {
                this.CallbackQueue.Enqueue(callback);       
            }
        }

        /// <summary>
        /// Execute all received callbacks.
        /// </summary>
        /// <remarks>
        /// This method is normally called from the main game thread.
        /// </remarks>
        public void ProcessCallbacks()
        {
            lock (this.CallbackQueue)
            {
                foreach (var callback in this.CallbackQueue)
                {
                    // Invoke the call back and signal invocation was completed
                    callback.Callback.Invoke();
                    callback.WasExecuted.Set();
                }
                
                this.CallbackQueue.Clear();
            }
        }
    }
}