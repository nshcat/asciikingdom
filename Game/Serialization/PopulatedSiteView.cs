namespace Game.Serialization
{
    /// <summary>
    /// Base class for populated site views
    /// </summary>
    public abstract class PopulatedSiteView : SiteView
    {
        public int Population { get; set; }
    }
}