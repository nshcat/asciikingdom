using Engine.Core;
using Engine.Graphics;

namespace Game.Data
{
    /// <summary>
    /// Represents the game world.
    /// </summary>
    public partial class World
    {
        /// <summary>
        /// The size of the map, in terrain tiles.
        /// </summary>
        public Size Dimensions { get; }

        /// <summary>
        /// The size of the overview map, in terrain tiles.
        /// </summary>
        public Size OverviewDimensions => this.Dimensions * this.OverviewScale;

        /// <summary>
        /// The relative scale of the overview map in relation to the detailed map
        /// </summary>
        public float OverviewScale { get; }
        
        /// <summary>
        /// The map the game is played on, full resolution.
        /// </summary>
        public TerrainType[,] DetailMap { get; } 
        
        /// <summary>
        /// The map used as an overview for orientation purposes. Each tile is the "average" terrain type
        /// based on 5x5 block in the detail map.
        /// </summary>
        public TerrainType[,] OverviewMap { get; }
        
        /// <summary>
        /// Rendering of the detailed map
        /// </summary>
        public Tile[,] DetailMapTiles { get; }
        
        /// <summary>
        /// Rendering of the overview map
        /// </summary>
        public Tile[,] OverviewMapTiles { get; }

        /// <summary>
        /// Internal constructor
        /// </summary>
        private World(Size dimensions, float overviewScale = 0.05f)
        {
            this.OverviewScale = overviewScale;
            this.Dimensions = dimensions;
            this.DetailMap = new TerrainType[this.Dimensions.Width, this.Dimensions.Height];
            this.DetailMapTiles = new Tile[this.Dimensions.Width, this.Dimensions.Height];
            this.OverviewMap = new TerrainType[this.OverviewDimensions.Width, this.OverviewDimensions.Height];
            this.OverviewMapTiles = new Tile[this.OverviewDimensions.Width, this.OverviewDimensions.Height];
        }
    }
}