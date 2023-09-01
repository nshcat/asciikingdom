using Engine.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Simulation.Worlds
{
    /// <summary>
    /// World layer composed of raw float values
    /// </summary>
    public class RawWorldLayer
        : WorldLayer
    {
        /// <summary>
        /// The array containing the raw float values.
        /// </summary>
        public float[,] Values { get; set; }

        /// <summary>
        /// Construct empty raw world layer.
        /// </summary>
        public RawWorldLayer(Size dimensions, string id, string name = "", bool dontAllocate = false)
            : base(WorldLayerType.RawFloat, dimensions, id, name)
        {
            if (!dontAllocate)
            {
                Values = new float[dimensions.Width, dimensions.Height];
            }
        }

        /// <summary>
        /// Determine average float value in given area
        /// </summary>
        protected override void ApplyOverviewValue(WorldLayer overviewLayer, Position overviewPosition, Rectangle sourceArea)
        {
            // This will only work if the overview layer has the same type as us.
            if (overviewLayer.Type != WorldLayerType.RawFloat)
                throw new ArgumentException("Given overview layer has wrong type");

            var entries = new List<float>();

            for (var iix = sourceArea.TopLeft.X; iix <= sourceArea.BottomRight.X; ++iix)
            {
                for (var iiy = sourceArea.TopLeft.Y; iiy <= sourceArea.BottomRight.Y; ++iiy)
                {
                    var element = Values[iix, iiy];
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

            (overviewLayer as RawWorldLayer).Values[overviewPosition.X, overviewPosition.Y] = average;
        }

        /// <summary>
        /// Write raw float data to given binary writer
        /// </summary>
        protected override void Serialize(BinaryWriter writer)
        {
            for (var ix = 0; ix < Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < Dimensions.Height; ++iy)
                {
                    writer.Write(Values[ix, iy]);
                }
            }
        }

        /// <summary>
        /// Read raw float data from given binary reader
        /// </summary>
        protected override void Deserialize(BinaryReader reader)
        {
            for (var ix = 0; ix < Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < Dimensions.Height; ++iy)
                {
                    Values[ix, iy] = reader.ReadSingle();
                }
            }
        }
    }
}
