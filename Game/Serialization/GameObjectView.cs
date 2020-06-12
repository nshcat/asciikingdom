using System;

namespace Game.Serialization
{
    /// <summary>
    /// Base class for game object views
    /// </summary>
    public abstract class GameObjectView
    {
        public Guid Id { get; set; }
    }
}