using System.Collections.Generic;
using Engine.Core;
using Game.Data;

namespace Game.Simulation
{
    /// <summary>
    /// Represents the detailed game world map. This is derived from the general <see cref="Map"/> class in order to
    /// support special map features that aren't rendered on the overview map, such as rivers and resources.
    /// </summary>
    public class DetailedMap : Map
    {
        /// <summary>
        /// Extra information about river terrain tiles, such as direction and size.
        /// </summary>
        public Dictionary<Position, RiverTileInfo> RiverTileInfo { get; set; }
        
        /// <summary>
        /// Resources present on the map
        /// </summary>
        public Dictionary<Position, ResourceType> Resources { get; set; }
        
        /// <summary>
        /// All provinces of the kingdom
        /// </summary>
        public List<Province> Provinces { get; set; }
            = new List<Province>();

        /// <summary>
        /// Construct a new detailed map instance
        /// </summary>
        public DetailedMap(Size dimensions, int seed)
            : base(dimensions, seed)
        {
            
        }

        /// <summary>
        /// Create tiles for this maps terrain and special features such as rivers and resources
        /// </summary>
        public override void UpdateTiles()
        {
            // First create tiles for base terrain
            base.UpdateTiles();
            
            // Render special map elements
            for (var ix = 0; ix < Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < Dimensions.Height; ++iy)
                {
                    var terrainType = this.Terrain[ix, iy];

                    if (terrainType == TerrainType.River)
                    {
                        var position = new Position(ix, iy);
                        
                        if (this.RiverTileInfo.ContainsKey(position))
                        {
                            var info = this.RiverTileInfo[position];
                            var tile = info.GetTile();
                            this.Tiles[ix, iy] = tile;
                        }
                    }
                }
            }
        }
    }
}