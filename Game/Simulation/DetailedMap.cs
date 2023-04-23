using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Engine.Core;
using Game.Data;
using Game.Maths;
using Game.Serialization;

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
            var worldDataPath = Path.Combine(prefix, "worlddata.json");

            var root = new JsonObject();

            // First, the two main layers
            var terrainLayerNode = this.TerrainLayer.Serialize();
            root.Add("terrain_layer", terrainLayerNode);

            var terrainTileLayerNode = this.TerrainTileLayer.Serialize();
            root.Add("terrain_tile_layer", terrainTileLayerNode);

            // Then the rest of the layers
            var layers = new JsonArray();

            foreach(var kvp in this.Layers)
            {
                var layerNode = kvp.Value.Serialize();
                layers.Add(layerNode);
            }

            root.Add("layers", layers);

            // Save river data
            var riverData = new JsonArray();

            foreach(var kvp in this.RiverTileInfo)
            {
                var riverDataNode = new SerializationHelper();
                riverDataNode.WriteValue<int>("x", kvp.Key.X);
                riverDataNode.WriteValue<int>("y", kvp.Key.Y);
                riverDataNode.WriteValue<int>("size", kvp.Value.Size);
                riverDataNode.WriteEnum("type", kvp.Value.Type);
                riverData.Add(riverDataNode.Node);        
            }

            root.Add("river_data", riverData);

            // Save discovery data
            root.Add("discovered", this.Discovered.Serialize());

            File.WriteAllText(worldDataPath, root.ToJsonString(Serialization.Serialization.DefaultOptions));
        }

        /// <summary>
        /// Load detailed map from given world prefix
        /// </summary>
        /// <param name="prefix"></param>
        public void Load(string prefix)
        {
            var worldDataPath = Path.Combine(prefix, "worlddata.json");
            var root = JsonNode.Parse(File.ReadAllText(worldDataPath)).AsObject();
            var helper = new DeserializationHelper(root);

            // Read the two main layers
            this.TerrainLayer = WorldLayer.Deserialize(helper.GetObject("terrain_layer").Node, this.Dimensions).As<TerrainWorldLayer>();
            this.TerrainTileLayer = WorldLayer.Deserialize(helper.GetObject("terrain_tile_layer").Node, this.Dimensions).As<TileWorldLayer>();

            // Read all other layers
            var layers = helper.GetArray("layers");

            foreach(var layerNode in layers)
            {
                var layer = WorldLayer.Deserialize(layerNode, this.Dimensions);
                this.Layers.Add(layer.Id, layer);
            }

            // Load river data
            var rivers = helper.GetArray("river_data");

            foreach(var riverNode in rivers)
            {
                var riverObj = new DeserializationHelper(riverNode.AsObject());
                var x = riverObj.ReadValue<int>("x");
                var y = riverObj.ReadValue<int>("y");
                var size = riverObj.ReadValue<int>("size");
                var type = riverObj.ReadEnum<RiverTileType>("type");
                this.RiverTileInfo.Add(new Position(x, y), new Data.RiverTileInfo(type, size));
            }

            // Load discovered data
            var discovered = helper.GetObject("discovered");
            this.Discovered.Deserialize(discovered.Node);
        }
    }
}