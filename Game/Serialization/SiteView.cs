
using Engine.Core;

namespace Game.Serialization
{
    /// <summary>
    /// Base class for site views
    /// </summary>
    public abstract class SiteView : GameObjectView
    {
        public string Name { get; set; }
        public Position Position { get; set; }
    }
}