using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Engine.Core;
using Game.Data;
using Game.Maths;
using Game.Serialization;
using SixLabors.Shapes;

namespace Game.Simulation.Worlds
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
            = new Dictionary<Position, RiverTileInfo>();

        /// <summary>
        /// Construct a new detailed map instance
        /// </summary>
        public DetailedMap(Size dimensions, int seed)
            : base(dimensions, seed)
        {
        }

        /// <summary>
        /// Save detailed map to given world prefix
        /// </summary>
        public void Save(string prefix)
        {
            // Write all layers to json
            var worldDataPath = System.IO.Path.Combine(prefix, "worlddata.json.gz");

            var root = new JsonObject();

            // First, the two main layers
            var terrainLayerNode = TerrainLayer.Serialize();
            root.Add("terrain_layer", terrainLayerNode);

            var terrainTileLayerNode = TerrainTileLayer.Serialize();
            root.Add("terrain_tile_layer", terrainTileLayerNode);

            // Then the rest of the layers
            var layers = new JsonArray();

            foreach (var kvp in Layers)
            {
                var layerNode = kvp.Value.Serialize();
                layers.Add(layerNode);
            }

            root.Add("layers", layers);

            // Save river data
            var riverData = new JsonArray();

            foreach (var kvp in RiverTileInfo)
            {
                var riverDataNode = new SerializationHelper();
                riverDataNode.WriteValue("x", kvp.Key.X);
                riverDataNode.WriteValue("y", kvp.Key.Y);
                riverDataNode.WriteValue("size", kvp.Value.Size);
                riverDataNode.WriteEnum("type", kvp.Value.Type);
                riverData.Add(riverDataNode.Node);
            }

            root.Add("river_data", riverData);

            // Save discovery data
            root.Add("discovered", Discovered.Serialize());

            // Compress JSON text to save disk space
            using(var outFile = File.Create(worldDataPath, 1024))
            {
                using(var compressStream = new GZipStream(outFile, CompressionLevel.Optimal))
                {
                    compressStream.Write(Encoding.UTF8.GetBytes(root.ToJsonString(Serialization.Serialization.DefaultOptions)));
                }
            }
        }

        /// <summary>
        /// Load detailed map from given world prefix
        /// </summary>
        /// <param name="prefix"></param>
        public void Load(string prefix)
        {
            var worldDataPath = System.IO.Path.Combine(prefix, "worlddata.json.gz");

            var worldDataContent = "";

            // Open compressed world data JSON file and decompress JSON text
            using (var worldDataFile = File.OpenRead(worldDataPath))
            {
                using (var uncompressStream = new GZipStream(worldDataFile, CompressionMode.Decompress))
                {
                    using (var bufferStream = new MemoryStream())
                    {
                        uncompressStream.CopyTo(bufferStream);
                        worldDataContent = Encoding.UTF8.GetString(bufferStream.ToArray());
                    }
                }
            }

            var root = JsonNode.Parse(worldDataContent).AsObject();
            var helper = new DeserializationHelper(root);

            // Read the two main layers
            TerrainLayer = WorldLayer.Deserialize(helper.GetObject("terrain_layer").Node, Dimensions).As<TerrainWorldLayer>();
            TerrainTileLayer = WorldLayer.Deserialize(helper.GetObject("terrain_tile_layer").Node, Dimensions).As<TileWorldLayer>();

            // Read all other layers
            var layers = helper.GetArray("layers");

            foreach (var layerNode in layers)
            {
                var layer = WorldLayer.Deserialize(layerNode, Dimensions);
                Layers.Add(layer.Id, layer);
            }

            // Load river data
            var rivers = helper.GetArray("river_data");

            foreach (var riverNode in rivers)
            {
                var riverObj = new DeserializationHelper(riverNode.AsObject());
                var x = riverObj.ReadValue<int>("x");
                var y = riverObj.ReadValue<int>("y");
                var size = riverObj.ReadValue<int>("size");
                var type = riverObj.ReadEnum<RiverTileType>("type");
                RiverTileInfo.Add(new Position(x, y), new RiverTileInfo(type, size));
            }

            // Load discovered data
            var discovered = helper.GetObject("discovered");
            Discovered.Deserialize(discovered.Node);
        }
    }
}