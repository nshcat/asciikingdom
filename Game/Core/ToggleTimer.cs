namespace Game.Core
{
    /// <summary>
    /// Delegate used for the timer toggled event
    /// </summary>
    public delegate void TimerToggleHandler(bool flag);
    
    /// <summary>
    /// Timer that toggles a flag on each tick event.
    /// </summary>
    public class ToggleTimer : Timer
    {
        /// <summary>
        /// Event that is fired every time the flag managed by this timer is toggled.
        /// </summary>
        public event TimerToggleHandler Toggled;
        
        /// <summary>
        /// The flag that is toggled by the timer
        /// </summary>
        public bool Flag { get; protected set; }

        /// <summary>
        /// Construct a new toggle timer instance with given initial value.
        /// </summary>
        /// <param name="initialValue">Initial value of the flag</param>
        public ToggleTimer(double period, bool initialValue = false)
            : base(period)
        {
            this.Flag = initialValue;
        }

        /// <summary>
        /// Implement toggling logic
        /// </summary>
        protected override void FireEvent()
        {
            base.FireEvent();

            this.Flag = !this.Flag;
            this.Toggled?.Invoke(this.Flag);
        }
    }
}