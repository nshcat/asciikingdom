namespace Game.Core
{
    /// <summary>
    /// Delegate used for timer events
    /// </summary>
    public delegate void TimerTickHandler();
    
    /// <summary>
    /// Implements a timer that fires an event after each period.
    /// </summary>
    /// <remarks>
    /// This class is particularly useful to implement animations.
    /// </remarks>
    public class Timer : ILogic
    {
        /// <summary>
        /// Event fired every time the timer period is exceeded
        /// </summary>
        public event TimerTickHandler Tick;
        
        /// <summary>
        /// The length of a single timer period, in seconds.
        /// </summary>
        /// <remarks>
        /// This value can not be changed after timer construction.
        /// </remarks>
        public double Period { get; protected set; }
        
        /// <summary>
        /// Accumulated carryover time
        /// </summary>
        protected double CarryoverTime { get; set; }

        /// <summary>
        /// Construct a new timer instance with given period.
        /// </summary>
        public Timer(double period)
        {
            this.Period = period;
        }

        /// <summary>
        /// Update timer with given elapsed time.
        /// </summary>
        public void Update(double deltaTime)
        {
            this.CarryoverTime += deltaTime;

            while (this.CarryoverTime >= this.Period)
            {
                this.CarryoverTime -= this.Period;
                this.FireEvent();
            }
        }

        /// <summary>
        /// Called by the update method when a timer event needs to be fired.
        /// </summary>
        /// <remarks>
        /// This virtual method is provided in order to allow derived classes to change
        /// timer event firing mechanics.
        /// </remarks>
        protected virtual void FireEvent()
        {
            this.Tick?.Invoke();
        }
    }
}