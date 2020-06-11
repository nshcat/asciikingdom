using System;

namespace Game.Simulation
{
    /// <summary>
    /// Represents an entity in the game simulation.
    /// </summary>
    public interface IGameObject
    {
        /// <summary>
        /// A unique identifier for this game object
        /// </summary>
        /// <remarks>
        /// This is mostly used for de/serialization.
        /// </remarks>
        public Guid Id { get; set; }

        /// <summary>
        /// Update the state of this game object based on the given number of elapsed weeks.
        /// </summary>
        /// <param name="weeks">Number of weeks elapsed since last update</param>
        public void Update(int weeks);
    }
}