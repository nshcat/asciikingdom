using Engine.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using Game.Serialization;
using YamlDotNet.Serialization.NodeTypeResolvers;
using System.Runtime.CompilerServices;

namespace Game.Simulation.Worlds
{
    /// <summary>
    /// Available types of world layers
    /// </summary>
    public enum WorldLayerType
    {
        /// <summary>
        /// 2D array of raw float values
        /// </summary>
        RawFloat,

        /// <summary>
        /// 2D array of terrain type enumeration values
        /// </summary>
        Terrain,

        /// <summary>
        /// 2D array of terrain tiles
        /// </summary>
        Tiles
    }

    /// <summary>
    /// Base classd for world layers, data structures containing a specific tile information
    /// of the world
    /// </summary>
    public abstract class WorldLayer
    {
        #region Static Fields
        /// <summary>
        /// Map from world layer type to the actual class type implementing that layer kind
        /// </summary>
        private static Dictionary<WorldLayerType, Type> _layerTypeMap
            = new Dictionary<WorldLayerType, Type> {
                { WorldLayerType.RawFloat, typeof(RawWorldLayer) },
                { WorldLayerType.Terrain, typeof(TerrainWorldLayer) },
                { WorldLayerType.Tiles, typeof(TileWorldLayer) }
            };
        #endregion

        #region Properties
        /// <summary>
        /// Type of this world layer
        /// </summary>
        public WorldLayerType Type { get; protected set; }

        /// <summary>
        /// Dimensions of this world layer
        /// </summary>
        public Size Dimensions { get; protected set; }

        /// <summary>
        /// Identifier of this world layer
        /// </summary>
        public string Id { get; set; }
            = "";

        /// <summary>
        /// Human-readable name of this world layer
        /// </summary>
        public string Name { get; set; }
            = "";
        #endregion

        #region Constructors
        /// <summary>
        /// Create empty world layer of given type, with given dimensions.
        /// </summary>
        public WorldLayer(WorldLayerType type, Size dimensions, string id, string name = "")
        {
            Type = type;
            Dimensions = dimensions;
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Protected constructor used for deserialization
        /// </summary>
        /// <param name="dimensions"></param>
        protected WorldLayer(Size dimensions)
        {

        }
        #endregion

        #region Helpers
        /// <summary>
        /// Cast down to specific world layer type
        /// </summary>
        public T As<T>() where T : WorldLayer
        {
            return this as T;
        }

        /// <summary>
        /// Determine average value inside given source area and apply as overview layer value at given position
        /// </summary>
        protected abstract void ApplyOverviewValue(WorldLayer overviewLayer, Position overviewPosition, Rectangle sourceArea);

        /// <summary>
        /// Create smaller, overview variant of this layer with given size factor
        /// </summary>
        /// <param name="factor">Factor determining the size of the smaller overview variant. Has to be in (0, 1.0).</param>
        public WorldLayer CreateOverview(float factor)
        {
            if (factor <= 0.0f || factor >= 1.0f)
                throw new ArgumentException("Invalid overview layer factor");

            // Calculate the size of the overview version of this layer
            var overviewDimensions = Dimensions * factor;

            // Get our actual type
            var layerType = GetType();

            // Create new overview layer
            var overviewLayer = (WorldLayer)Activator.CreateInstance(layerType, new object[] { overviewDimensions, Id, Name, false });

            // Fill with averaged data. This is done in the subclasses since we cant know how to average the data
            // here.
            var scaleFactor = (int)(1.0f / factor);
            for (var ix = 0; ix < overviewDimensions.Width; ++ix)
            {
                for (var iy = 0; iy < overviewDimensions.Height; ++iy)
                {
                    var topLeft = new Position(ix * scaleFactor, iy * scaleFactor);
                    var bottomRight = new Position(topLeft.X + 4, topLeft.Y + 4);

                    ApplyOverviewValue(overviewLayer, new Position(ix, iy), new Rectangle(topLeft, bottomRight));
                }
            }

            return overviewLayer;
        }
        #endregion

        #region De/Serialization
        /// <summary>
        /// Write data to given binary stream writer.
        /// </summary>
        protected abstract void Serialize(BinaryWriter writer);

        /// <summary>
        /// Read data from given binary stream reader
        /// </summary>
        protected abstract void Deserialize(BinaryReader reader);

        /// <summary>
        /// Serialize world layer to JSON node. Binary data is stored encoded in base64.
        /// </summary>
        public JsonNode Serialize()
        {
            var helper = new SerializationHelper();

            // Serialize properties
            helper.WriteValue("id", Id);
            helper.WriteValue("name", Name);
            helper.WriteEnum("type", Type);

            // Write actual data as blob
            using (var buffer = new MemoryStream())
            {
                using (var writer = new BinaryWriter(buffer))
                {
                    Serialize(writer);
                }

                var data = buffer.GetBuffer();
                var dataString = Convert.ToBase64String(data);
                helper.WriteValue("data", dataString);
            }

            return helper.Node;
        }

        /// <summary>
        /// Deserialize world layer from given JSON node 
        /// </summary>
        public static WorldLayer Deserialize(JsonNode node, Size dimensions)
        {
            var helper = new DeserializationHelper(node.AsObject());

            // Read properties
            var id = helper.ReadValue<string>("id");
            var name = helper.ReadValue<string>("name");
            var type = helper.ReadEnum<WorldLayerType>("type");

            // Create empty world layer object of correct type
            if (!_layerTypeMap.ContainsKey(type))
                throw new Exception("Unknown world layer type encountered while deserializing world layer");

            var classType = _layerTypeMap[type];
            var layer = (WorldLayer)Activator.CreateInstance(classType, new object[] { dimensions, id, name, false });

            // Read actual data
            var dataString = helper.ReadValue<string>("data");
            var data = Convert.FromBase64String(dataString);
            using (var buffer = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(buffer))
                {
                    layer.Deserialize(reader);
                }
            }

            return layer;
        }
        #endregion
    }
}
