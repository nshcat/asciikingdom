using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Graphics;

namespace Game.Simulation.Modules
{
    /// <summary>
    /// Delegate used for PopulationChanged event in <see cref="PopulationController"/>
    /// </summary>
    /// <param name="source">The event source</param>
    /// <param name="newPopulation">The new population value</param>
    public delegate void PopulationChangedHandler(PopulationController source, int newPopulation);
    
    /// <summary>
    /// A site module that handles population growth
    /// </summary>
    public class PopulationController : SiteModule
    {
        /// <summary>
        /// Event that is fired every time the population number changes
        /// </summary>
        public event PopulationChangedHandler PopulationChanged;
        
        /// <summary>
        /// The current population count
        /// </summary>
        public int Population { get; set; }

        /// <summary>
        /// Retrieve the current label for the associated site based on the current population count.
        /// </summary>
        public string CurrentLabel => this.GetCurrentLabel();

        /// <summary>
        /// Retrieve the current tile for the associated site based on the current population count.
        /// </summary>
        public Tile CurrentTile => this.GetCurrentTile();
        
        /// <summary>
        /// All growth stages for the associated site
        /// </summary>
        public List<SiteGrowthStage> GrowthStages { get; protected set; }
        
        /// <summary>
        /// Create new population controller
        /// </summary>
        public PopulationController(
            WorldSite parentSite,
            IEnumerable<SiteGrowthStage> stages,
            int initialPopulation
        ) 
            : base(parentSite)
        {
            this.GrowthStages = stages.OrderBy(x => x.PopulationThreshold).ToList();
            this.Population = initialPopulation;
        }

        public override void Update(int weeks)
        {
            // Do nothing
        }

        /// <summary>
        /// Calculate the current site label based on the current population count
        /// </summary>
        protected string GetCurrentLabel()
        {
            if(this.GrowthStages.Count <= 0)
                throw new InvalidOperationException("No growth stages associated with this population controller");
            
            var label = "";

            foreach (var stage in this.GrowthStages)
            {
                if (this.Population >= stage.PopulationThreshold)
                    label = stage.Descriptor;
                else
                {
                    return label;
                }
            }

            return label;
        }

        /// <summary>
        /// Calculate the current site tile based on the current population count
        /// </summary>
        protected Tile GetCurrentTile()
        {
            if(this.GrowthStages.Count <= 0)
                throw new InvalidOperationException("No growth stages associated with this population controller");
            
            Tile tile = new Tile();
            
            foreach (var stage in this.GrowthStages)
            {
                if (this.Population >= stage.PopulationThreshold)
                    tile = stage.Tile;
                else
                {
                    return tile;
                }
            }

            return tile;
        }
    }
}