using Engine.Core;
using Engine.Graphics;
using Game.Core;
using Game.Data;
using Game.Simulation.Worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Ui
{
    /// <summary>
    /// Map overlay that shows fertility information for a given crop type
    /// </summary>
    class FertilityOverlay : MapOverlay
    {
        /// <summary>
        /// The crop the fertility data is currently shown for
        /// </summary>
        public CropType Crop { get; set; }

        /// <summary>
        /// Create new fertility overlay instance
        /// </summary>
        /// <param name="crop">Crop to show fertility data for</param>
        public FertilityOverlay(CropType crop)
        {
            this.Blinking = true;
            this.Crop = crop;
        }

        public override Optional<Tile> DetermineTile(Map world, Position worldPosition)
        {
            if (!world.IsDiscovered(worldPosition))
                return Optional<Tile>.Empty;

            // If the world is ocean or hills at this position, nothing can grow
            var terrain = world.TerrainLayer.Values[worldPosition.X, worldPosition.Y];

            if (!TerrainTypeData.AcceptsCrops(terrain))
                return Optional<Tile>.Empty;

            var X = worldPosition.X;
            var Y = worldPosition.Y;
            var temperature = world.GetLayer<RawWorldLayer>("raw_temperature").Values[X, Y];
            var drainage = world.GetLayer<RawWorldLayer>("raw_drainage").Values[X, Y];
            var rainfall = world.GetLayer<RawWorldLayer>("raw_rainfall").Values[X, Y];

            var fertility = this.Crop.FertilityFactors.CalculateFertilityFactor(
                temperature, drainage, rainfall
            );

            return Optional<Tile>.Of(this.MakeTile(fertility));
        }

        /// <summary>
        /// Create colored tile for given value in [0, 1]
        /// </summary>
        protected Tile MakeTile(float value)
        {
            var color = Color.Lerp(value, DefaultColors.Red, DefaultColors.Green);

            return new Tile(0, DefaultColors.Black, color);
        }
    }
}
