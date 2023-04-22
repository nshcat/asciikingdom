using Engine.Core;
using Game.Data;
using Game.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Utility
{
    /// <summary>
    /// Static helper class that allows generation of a terrain tile layer
    /// from a terrain type layer
    /// </summary>
    public static class TerrainTileLayerGenerator
    {
        /// <summary>
        /// Create a tile world layer from given terrain type layer
        /// </summary>
        public static TileWorldLayer CreateTileLayer(int seed, TerrainWorldLayer terrainLayer,
            Dictionary<Position, RiverTileInfo> riverData = null)
        {
            var random = new Random(seed + 166554);
            var targetLayer = new TileWorldLayer(terrainLayer.Dimensions, "terrain_tiles", "Terrain Tiles");

            for (var ix = 0; ix < terrainLayer.Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < terrainLayer.Dimensions.Height; ++iy)
                {
                    var position = new Position(ix, iy);
                    var terrainType = terrainLayer.Values[ix, iy];

                    if (terrainType == TerrainType.River && riverData != null)
                    {
                        var info = riverData[position];
                        var tile = info.GetTile();
                        targetLayer.Values[ix, iy] = tile;
                    }
                    else
                    {
                        var info = TerrainTypeData.GetInfo(terrainType);
                        var tile = info.PickTile(random);
                        targetLayer.Values[ix, iy] = tile;
                    }
                }
            }

            return targetLayer;
        }
    }
}
