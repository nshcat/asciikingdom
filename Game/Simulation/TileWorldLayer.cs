using Engine.Core;
using Engine.Graphics;
using Game.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Simulation
{
    /// <summary>
    /// World layer consisting of tiles
    /// </summary>
    public class TileWorldLayer
        : WorldLayer
    {
        /// <summary>
        /// Array of tile data
        /// </summary>
        public Tile[,] Values { get; set; }

        /// <summary>
        /// Construct new terrain world layer
        /// </summary>
        public TileWorldLayer(Size dimensions, string id, string name = "", bool dontAllocate = false)
            : base(WorldLayerType.Tiles, dimensions, id, name)
        {
            if (!dontAllocate)
            {
                this.Values = new Tile[dimensions.Width, dimensions.Height];
            }
        }

        /// <summary>
        /// Determine average tile value in given area
        /// </summary>
        protected override void ApplyOverviewValue(WorldLayer overviewLayer, Position overviewPosition, Rectangle sourceArea)
        {
            // This will only work if the overview layer has the same type as us.
            if (overviewLayer.Type != WorldLayerType.Tiles)
                throw new ArgumentException("Given overview layer has wrong type");

            var entries = new List<Tile>();

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

            (overviewLayer as TileWorldLayer).Values[overviewPosition.X, overviewPosition.Y] = average;
        }

        /// <summary>
        /// Write tile data to given binary writer
        /// </summary>
        protected override void Serialize(BinaryWriter writer)
        {
            for (var ix = 0; ix < Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < Dimensions.Height; ++iy)
                {
                    var tile = this.Values[ix, iy];
                    writer.Write(tile.Glyph);
                    writer.Write((byte)tile.Front.R);
                    writer.Write((byte)tile.Front.G);
                    writer.Write((byte)tile.Front.B);
                    writer.Write((byte)tile.Back.R);
                    writer.Write((byte)tile.Back.G);
                    writer.Write((byte)tile.Back.B);
                }
            }
        }

        /// <summary>
        /// Read tile data from given binary reader
        /// </summary>
        protected override void Deserialize(BinaryReader reader)
        {
            for (var ix = 0; ix < Dimensions.Width; ++ix)
            {
                for (var iy = 0; iy < Dimensions.Height; ++iy)
                {
                    var glyph = reader.ReadInt32();
                    var front = new Color(
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte()
                    );
                    var back = new Color(
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte()
                    );

                    this.Values[ix, iy] = new Tile(glyph, front, back);
                }
            }
        }
    }
}
