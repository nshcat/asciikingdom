using System;
using System.Threading;
using OpenToolkit.Graphics.OpenGL;

namespace Game.Core
{
    /// <summary>
    /// Represents a potentially long-running operation that is to be executed in the background, but is given to notify
    /// the main game thread of arbitrary events.
    /// </summary>
    /// <remarks>
    /// To implement a custom async operation, this abstract class is to be derived, and <see cref="DoOperation"/> is to
    /// be overridden with the actual operation code. It's important to periodically check <see cref="AbortRequest"/>
    /// (for example using the convenience property <see cref="ShouldAbort"/>) in order to correctly handle abortion
    /// requests from the main game thread.
    /// </remarks>
    public abstract class AsyncOperation
    {
        /// <summary>
        /// Reference to the object that will receive callbacks generated by this operation
        /// </summary>
        protected AsyncCallbackReceiver CallbackReceiver { get; }

        /// <summary>
        /// Signals whether the operation thread should gracefully abort
        /// </summary>
        protected ManualResetEvent AbortRequest { get; } = new ManualResetEvent(false);

        /// <summary>
        /// Check whether the main game thread requested graceful abortion of the operation.
        /// </summary>
        /// <remarks>
        /// This is generally called by the async operation.
        /// </remarks>
        protected bool ShouldAbort => this.AbortRequest.WaitOne(0);

        /// <summary>
        /// Whether the operation is running.
        /// </summary>
        public bool IsRunning => (this.WorkerThread != null) && this.WorkerThread.IsAlive;

        /// <summary>
        /// Whether the operation has finished executing.
        /// </summary>
        public bool IsFinished => this.WasStarted && !this.IsRunning;
        
        /// <summary>
        /// Whether the operation was started.
        /// </summary>
        /// <remarks>
        /// This property is meant to be used by the main game thread and is required, because <see cref="IsRunning"/>
        /// equaling to false does not necessarily mean that the operation was never started.
        /// </remarks>
        public bool WasStarted { get; protected set; }

        /// <summary>
        /// Whether abortion of the operation was requested
        /// </summary>
        public bool WasAborted => this.ShouldAbort;
        
        /// <summary>
        /// Thread executing the async operation
        /// </summary>
        protected Thread WorkerThread { get; set; }

        /// <summary>
        /// Construct new async operation with given callback sink.
        /// </summary>
        protected AsyncOperation(AsyncCallbackReceiver callbackReceiver)
        {
            this.CallbackReceiver = callbackReceiver;
        }

        /// <summary>
        /// Construct new async operation, creating a new callback receiver.
        /// </summary>
        /// <remarks>
        /// This constructor is especially useful if the derived class would contain a callback receiver anyways,
        /// for example in order to convert signals from the async operation into events inside the game thread.
        /// </remarks>
        protected AsyncOperation()
        {
            this.CallbackReceiver = new AsyncCallbackReceiver();
        }

        /// <summary>
        /// Execute the asynchronous operation.
        /// </summary>
        public void Run()
        {
            if(this.WasStarted)
                throw new InvalidOperationException("Async operation is already running");

            this.WorkerThread = new Thread(this.DoOperation);
            this.WorkerThread.Start();
            this.WasStarted = true;
        }

        /// <summary>
        /// Signal to the operation that abortion was requested.
        /// </summary>
        /// <remarks>
        /// This method will not cause forced thread abortion, but rather allow the running operation
        /// to shut-down gracefully.
        /// </remarks>
        public void RequestAbort()
        {
            if(!this.IsRunning)
                throw new InvalidOperationException("Async operation is not running");
            
            this.AbortRequest.Set();
        }

        /// <summary>
        /// Forcefully abort the running operation.
        /// </summary>
        public void ForceAbort()
        {
            if(!this.IsRunning)
                throw new InvalidOperationException("Async operation is not running");
            
            this.WorkerThread.Abort();
        }

        /// <summary>
        /// Invoke given action via the <see cref="CallbackReceiver"/> reference on the main game thread, not waiting
        /// for its completion.
        /// </summary>
        protected void Invoke(Action action)
        {
            this.CallbackReceiver.BeginInvoke(new AsyncCallback(action));
        }

        /// <summary>
        /// Invoke given action via the <see cref="CallbackReceiver"/> reference on the main game thread, and wait
        /// for its completion.
        /// </summary>
        protected void InvokeWait(Action action)
        {
            var callback = new AsyncCallback(action);
            this.CallbackReceiver.BeginInvoke(callback);
            callback.WasExecuted.WaitOne();
        }
        
        /// <summary>
        /// Perform the actual operation.
        /// </summary>
        protected abstract void DoOperation();
    }
}