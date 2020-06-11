using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Engine.Core;
using Engine.Graphics;

namespace Game.Simulation
{
    /// <summary>
    /// Abstract base class for world sites harboring a population of citizens, that have needs, desires and moods.
    /// </summary>
    public abstract class PopulatedSite : IWorldSite
    {
        #region Abstract properties
        public abstract string Name { get; set; }
        public abstract Position Position { get; set; }
        #endregion
        
        #region Implemented interface properties
        public Guid Id { get; set; }
        
        public string TypeDescriptor => this.GetCurrentDescriptor();
        
        public Tile Tile => this.GetCurrentTile();
        #endregion
        
        /// <summary>
        /// Current population count
        /// </summary>
        public int Population { get; set; }
        
        /// <summary>
        /// List of population thresholds and associated strings used to determine the current
        /// site type descriptor, such as "town" vs "city".
        /// </summary>
        [JsonIgnore]
        protected List<(int, string)> SiteDescriptors { get; set; }
        
        /// <summary>
        /// List of population thresholds and associated tiles used to determine the current world map
        /// tile for this populated site
        /// </summary>
        [JsonIgnore]
        protected List<(int, Tile)> SiteTiles { get; set; }

        /// <summary>
        /// Base class constructor that sets the identifier to a new Guid
        /// </summary>
        /// <param name="stages">The different growth stages this site can be in</param>
        public PopulatedSite(IEnumerable<SiteGrowthStage> stages, int initialPopulation)
        {
            this.Id = Guid.NewGuid();
            this.SiteDescriptors = stages.Select(x => (x.PopulationThreshold, x.Descriptor)).ToList();
            this.SiteTiles = stages.Select(x => (x.PopulationThreshold, x.Tile)).ToList();
            this.Population = initialPopulation;
        }

        public abstract void Update(int weeks);

        /// <summary>
        /// Determine the current site type descriptor based on the current population count
        /// </summary>
        protected string GetCurrentDescriptor()
        {
            if(this.SiteDescriptors.Count <= 0)
                throw new InvalidOperationException("Populate site descriptor list is empty");

            var result = "";
            
            foreach (var (threshold, descriptor) in this.SiteDescriptors)
            {
                if (this.Population >= threshold)
                    result = descriptor;
            }

            return result;
        }
        
        /// <summary>
        /// Determine the current site tile based on the current population count
        /// </summary>
        protected Tile GetCurrentTile()
        {
            if(this.SiteTiles.Count <= 0)
                throw new InvalidOperationException("Populate site tile list is empty");

            var result = Tile.Empty;
            
            foreach (var (threshold, tile) in this.SiteTiles)
            {
                if (this.Population >= threshold)
                    result = tile;
            }

            return result;
        }
    }
}