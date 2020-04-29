using Engine.Core;
using Engine.Graphics;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;

namespace Engine.Rendering
{
    /// <summary>
    /// Represents the shader program used to render ASCII surfaces.
    /// </summary>
    public partial class AsciiMaterial
    {
        /// <summary>
        /// The underlying shader program object.
        /// </summary>
        protected ShaderProgram Program { get; set; }

        /// <summary>
        /// Fog density value. Affects how fast the fog thickens with increasing depth values.
        /// </summary>
        public float FogDensity { get; set; } = 0.15f;
        
        /// <summary>
        /// The color of the fog at maximum thickness.
        /// </summary>
        public Vector4 FogColor { get; set; } = new Vector4(0.1f, 0.1f, 0.3f, 1.0f);
        
        /// <summary>
        /// The dimensions of the surface using this material, in glyphs (not pixels!)
        /// </summary>
        public Size SurfaceDimensions { get; set; }

        /// <summary>
        /// Information about the current tileset used by the surface rendered with this material.
        /// </summary>
        public TilesetProperties TilesetInfo { get; set; }
        
        /// <summary>
        /// Absolute position of the top left corner of the surface on the device screen, in pixels
        /// </summary>
        public Position TopLeft { get; set; }
        
        /// <summary>
        /// Create a new instance of this material.
        /// </summary>
        public AsciiMaterial()
        {
            this.Program = new ShaderProgram(
                Shader.FromText(ShaderType.FragmentShader, this.FragmentShaderSource),
                Shader.FromText(ShaderType.VertexShader, this.VertexShaderSource)
            );
        }

        /// <summary>
        /// Activate and bind this material
        /// </summary>
        public void Use()
        {
            this.Program.Use();
        }

        /// <summary>
        /// Set all material uniforms to prepare for rendering.
        /// </summary>
        /// <param name="renderParams">Rendering matrices to use</param>
        public void ApplyParameters(RenderParams renderParams)
        {
            // Set view and projection matrices
            this.Program.Projection = renderParams.Projection;
            
            // Upload uniforms
            this.Program.SetInt("tex", 0);
            this.Program.SetInt("shadow_tex", 1);
            this.Program.SetInt("input_buffer", 2);
            
            this.Program.SetVec4F("fog_color", this.FogColor);
            this.Program.SetInt("surface_width", this.SurfaceDimensions.Width);
            this.Program.SetInt("sheet_width", this.TilesetInfo.TilesetDimensions.Width);
            this.Program.SetInt("sheet_height", this.TilesetInfo.TilesetDimensions.Height);

            this.Program.SetInt("glyph_width", this.TilesetInfo.BaseTileDimensions.Width);
            this.Program.SetInt("glyph_height", this.TilesetInfo.BaseTileDimensions.Height);
            
            this.Program.SetFloat("glyph_scaling_factor", this.TilesetInfo.ScaleFactor);
            this.Program.SetInt("position_x", this.TopLeft.X);
            this.Program.SetInt("position_y", this.TopLeft.Y);
            this.Program.SetFloat("fog_density", this.FogDensity);
        }
    }
}