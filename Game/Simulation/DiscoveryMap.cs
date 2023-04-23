using Engine.Core;
using Game.Data;
using Game.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Game.Simulation
{
    /// <summary>
    /// Map recording discovery information
    /// </summary>
    public class DiscoveryMap
    {
        /// <summary>
        /// Boolean discovery values
        /// </summary>
        public bool[,] Values { get; set; }

        /// <summary>
        /// Dimensions of the discovery map
        /// </summary>
        public Size Dimensions { get; set; }

        public DiscoveryMap(Size dimensions, bool dontAllocate = false)
        {
            this.Dimensions = dimensions;
            this.Values = new bool[dimensions.Width, dimensions.Height];
        }

        /// <summary>
        /// Create smaller, overview discovery map with given factor
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
        public DiscoveryMap CreateOverview(float factor)
        {
            if (factor <= 0.0f || factor >= 1.0f)
                throw new ArgumentException("Invalid overview layer factor");

            // Calculate the size of the overview version of this layer
            var overviewDimensions = this.Dimensions * factor;

            var overviewMap = new DiscoveryMap(overviewDimensions);

            var scaleFactor = (int)(1.0f / factor);
            List<bool> entries = new List<bool>();
            for (var ix = 0; ix < overviewDimensions.Width; ++ix)
            {
                for (var iy = 0; iy < overviewDimensions.Height; ++iy)
                {
                    entries.Clear();

                    var topLeft = new Position(ix * scaleFactor, iy * scaleFactor);
                    var bottomRight = new Position(topLeft.X + 4, topLeft.Y + 4);

                    var sourceArea = new Rectangle(topLeft, bottomRight);

                    for (var iix = sourceArea.TopLeft.X; iix <= sourceArea.BottomRight.X; ++iix)
                    {
                        for (var iiy = sourceArea.TopLeft.Y; iiy <= sourceArea.BottomRight.Y; ++iiy)
                        {
                            var element = this.Values[iix, iiy];
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

                    overviewMap.Values[ix, iy] = average;
                }
            }

            return overviewMap;
        }

        /// <summary>
        /// Serialize discovery data to JSON node
        /// </summary>
        public JsonNode Serialize()
        {
            var obj = new SerializationHelper();

            using(var memoryStream = new MemoryStream())
            {
                using(var writer = new BinaryWriter(memoryStream))
                {
                    for (var ix = 0; ix < Dimensions.Width; ++ix)
                    {
                        for (var iy = 0; iy < Dimensions.Height; ++iy)
                        {
                            writer.Write(this.Values[ix, iy] ? (byte)1 : (byte)0);
                        }
                    }
                }

                var dataStr = Convert.ToBase64String(memoryStream.GetBuffer());
                obj.WriteValue("data", dataStr);
            }

            return obj.Node;
        }

        /// <summary>
        /// Deserialize discovery node from JSON node
        /// </summary>
        public void Deserialize(JsonNode node)
        {
            var obj = new DeserializationHelper(node.AsObject());
            var dataStr = obj.ReadValue<string>("data");
            var buffer = Convert.FromBase64String(dataStr);

            using (var memoryStream = new MemoryStream(buffer))
            {
                using (var reader = new BinaryReader(memoryStream))
                {
                    for (var ix = 0; ix < Dimensions.Width; ++ix)
                    {
                        for (var iy = 0; iy < Dimensions.Height; ++iy)
                        {
                            this.Values[ix, iy] = (reader.ReadByte() == 1);
                        }
                    }
                }
            }
        }
    }
}
