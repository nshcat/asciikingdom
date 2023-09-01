using Engine.Core;
using Game.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Simulation.Worlds
{
    /// <summary>
    /// Terain type world layer
    /// </summary>
    public class TerrainWorldLayer
        : WorldLayer
    {
        /// <summary>
        /// Array of terrain type values
        /// </summary>
        public TerrainType[,] Values { get; set; }

        /// <summary>
        /// Construct new terrain world layer
        /// </summary>
        public TerrainWorldLayer(Size dimensions, string id, string name = "", bool dontAllocate = false)
            : base(WorldLayerType.Terrain, dimensions, id, name)
        {
            if (!dontAllocate)
            {
                Values = new TerrainType[dimensions.Width, dimensions.Height];
            }
        }

        /// <summary>
        /// Determine average terrain type in given area
        /// </summary>
        protected override void ApplyOverviewValue(WorldLayer overviewLayer, Position overviewPosition, Rectangle sourceArea)
        {
            // This will only work if the overview layer has the same type as us.
            if (overviewLayer.Type != WorldLayerType.Terrain)
                throw new ArgumentException("Given overview layer has wrong type");

            var entries = new List<TerrainType>();

            for (var iix = sourceArea.TopLeft.X; iix <= sourceArea.BottomRight.X; ++iix)
            {
                for (var iiy = sourceArea.TopLeft.Y; iiy <= sourceArea.BottomRight.Y; ++iiy)
                {
                    var element = Values[iix, iiy];

                    // Ignore rivers
                    if (element != TerrainType.River)
                        entries.Add(element);
                }
            }

            var average = entries
                .GroupBy(x => x)
                .Select(x => new
                {
                    Count = x.Count(),
                    Entry = x.Key
                })
                .OrderByDescending(x => x.Count)
                .Select(x => x.Entry)
                .First();

            (overviewLayer as TerrainWorldLayer).Values[overviewPosition.X, overviewPosition.Y] = average;
        }

        /// <summary>
        /// Write terrain data to given binary writer
        /// </summary>
        protected override void Serialize(BinaryWriter writer)
        {
            for (var ix = 0; ix < Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < Dimensions.Height; ++iy)
                {
                    writer.Write((byte)Values[ix, iy]);
                }
            }
        }

        /// <summary>
        /// Read terrain data from given binary reader
        /// </summary>
        protected override void Deserialize(BinaryReader reader)
        {
            for (var ix = 0; ix < Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < Dimensions.Height; ++iy)
                {
                    Values[ix, iy] = (TerrainType)reader.ReadByte();
                }
            }
        }
    }
}
