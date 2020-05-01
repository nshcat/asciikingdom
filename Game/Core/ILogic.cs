namespace Game.Core
{
    /// <summary>
    /// Interface for game elements that have internal logic that needs updating each frame
    /// </summary>
    public interface ILogic
    {
        /// <summary>
        /// Update internal state using given amount of seconds elapsed since last update
        /// </summary>
        /// <param name="deltaTime">Number of seconds elapsed since last update</param>
        public void Update(double deltaTime);
    }
}