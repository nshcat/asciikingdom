using Engine.Core;
using Game.Data;
using Game.Simulation.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.WorldGen
{
    /// <summary>
    /// World generation step that precalculates crop fertility for each crop type
    /// and stores them as layers
    /// </summary>
    public class CropFertilityCalculator
    {
        /// <summary>
        /// The generated, detailed map
        /// </summary>
        protected DetailedMap Map { get; }

        /// <summary>
        /// The rainfall component of the map
        /// </summary>
        protected RainfallMap Rainfall { get; }

        /// <summary>
        /// Layers containing the fertility values for each crop type
        /// </summary>
        protected Dictionary<string, RawWorldLayer> FertilityLayers { get; set; }
            = new Dictionary<string, RawWorldLayer>();

        public CropFertilityCalculator(DetailedMap map)
        {
            this.Map = map;
        }

        /// <summary>
        /// Generate crop fertilities for all known crop types
        /// </summary>
        public void Generate()
        {
            foreach(var cropTypeKvp in CropTypeManager.Instance.AllTypes)
            {
                var layerName = $"fertility_{cropTypeKvp.Key}";
                var fertilityFactors = cropTypeKvp.Value.FertilityFactors;
                var layer = new RawWorldLayer(this.Map.Dimensions, layerName, $"Fertility ({cropTypeKvp.Value.Name})");

                var tempLayer = this.Map.GetLayer<RawWorldLayer>("raw_temperature");
                var drainLayer = this.Map.GetLayer<RawWorldLayer>("raw_drainage");
                var rainLayer = this.Map.GetLayer<RawWorldLayer>("raw_rainfall");

                for (int ix = 0; ix < this.Map.Dimensions.Width; ++ix)
                {
                    for (int iy = 0; iy < this.Map.Dimensions.Height; ++iy)
                    {
                        layer[ix, iy] = fertilityFactors.CalculateFertilityFactor(tempLayer[ix, iy], drainLayer[ix, iy], rainLayer[ix, iy]);
                    }
                }

                this.FertilityLayers.Add(layerName, layer);
            }
        }

        /// <summary>
        /// Store pregenerated crop fertility layers in given map
        /// </summary>
        /// <param name="map"></param>
        public void StoreLayers()
        {
            foreach(var kvp in this.FertilityLayers)
            {
                this.Map.Layers.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
