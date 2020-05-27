using System.IO;
using Engine.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Engine.Resources
{
    /// <summary>
    /// A class managing resources used by the game.
    /// </summary>
    public class ResourceManager
    {
        /// <summary>
        /// Resource root directory
        /// </summary>
        public string Prefix { get; protected set; }

        /// <summary>
        /// Create a new resource manager based on given resource path prefix.
        /// </summary>
        /// <param name="prefix">Resource root</param>
        public ResourceManager(string prefix)
        {
            this.Prefix = prefix;
        }

        /// <summary>
        /// Create a new resource manager, using the current working directory for resources.
        /// The resource path will be ./Resources/
        /// </summary>
        public ResourceManager()
            : this(Path.Combine(Directory.GetCurrentDirectory(), "Resources"))
        {
            
        }

        /// <summary>
        /// Retrieve text file contents
        /// </summary>
        /// <param name="name">Name of file to retrieve contents from</param>
        /// <returns>Contents of specified text file</returns>
        public string GetText(string name)
        {
            var path = this.BuildPath("text", name);

            return File.ReadAllText(path);
        }

        /// <summary>
        /// Retrieve tileset
        /// </summary>
        /// <param name="name">Name of tileset to retrieve</param>
        /// <param name="scaleFactor">Requested scaling factor</param>
        /// <returns>Tileset instance</returns>
        public Tileset GetTileset(string name, float scaleFactor = 1.0f)
        {
            var texture = this.GetTexture(name);
            var shadows = this.GetTexture("shadows.png");
            
            return new Tileset(texture, shadows, scaleFactor);
        }

        /// <summary>
        /// Retrieve texture
        /// </summary>
        /// <param name="name">Name of texture to retrieve</param>
        /// <returns>Texture instance</returns>
        public Image<Rgba32> GetTexture(string name)
        {
            var path = this.BuildPath("textures", name);
            return (Image<Rgba32>)Image.Load(Configuration.Default, path);
        }

        /// <summary>
        /// Build an absolute resource path.
        /// </summary>
        /// <param name="name">Name of the resource file</param>
        /// <param name="subfolder">Name of the subfolder it resides in</param>
        /// <returns></returns>
        protected string BuildPath(string subfolder, string name)
        {
            return Path.Combine(this.Prefix, subfolder, name);
        }
    }
}