using System;
using Game.Data;

namespace Game.Simulation.Modules
{
    /// <summary>
    /// Site module that extracts raw resources from terrain tiles and special resources based on selected
    /// extraction recipes.
    /// </summary>
    public class ProductExtractor : SiteModule
    {
        /// <summary>
        /// The product storage the extracted products will end up in
        /// </summary>
        protected ProductStorage DestinationStorage { get; set; }
        
        /// <summary>
        /// Construct new product extract associated with given site.
        /// </summary>
        /// <remarks>
        /// The extractor will automatically try to retrieve the product storage instance associated
        /// with the given site, and will throw if no such module exists.
        /// </remarks>
        /// <param name="parentSite">Associated site. Has to contain a <see cref="ProductStorage"/> module.</param>
        public ProductExtractor(WorldSite parentSite)
            : base(parentSite)
        {
            // Try to retrieve reference to the product storage module
            if(!parentSite.HasModule<ProductStorage>())
                throw new ArgumentException("Associated site has no product storage module");

            this.DestinationStorage = parentSite.QueryModule<ProductStorage>();
        }

        public override void Update(int weeks)
        {
            // Do nothing (yet)
        }
    }
}