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
        public Size Dimensions { get; protected set; }

        /// <summary>
        /// The size of the overview map, in terrain tiles.
        /// </summary>
        public Size OverviewDimensions => this.Dimensions * 0.05f;
        
        /// <summary>
        /// The map the game is played on, full resolution.
        /// </summary>
        public TerrainType[,] DetailMap { get; protected set; } 
        
        /// <summary>
        /// The map used as an overview for orientation purposes. Each tile is the "average" terrain type
        /// based on 5x5 block in the detail map.
        /// </summary>
        public TerrainType[,] OverviewMap { get; protected set; }
        
        /// <summary>
        /// Rendering of the detailed map
        /// </summary>
        public Tile[,] DetailMapTiles { get; protected set; }
        
        /// <summary>
        /// Rendering of the overview map
        /// </summary>
        public Tile[,] OverviewMapTiles { get; protected set; }

        /// <summary>
        /// Internal constructor
        /// </summary>
        private World(Size dimensions)
        {
            this.Dimensions = dimensions;
            this.DetailMap = new TerrainType[this.Dimensions.Width, this.Dimensions.Height];
            this.DetailMapTiles = new Tile[this.Dimensions.Width, this.Dimensions.Height];
            this.OverviewMap = new TerrainType[this.OverviewDimensions.Width, this.OverviewDimensions.Height];
            this.OverviewMapTiles = new Tile[this.OverviewDimensions.Width, this.OverviewDimensions.Height];
        }
    }
}