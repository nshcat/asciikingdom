namespace Engine.Rendering
{
    /// <summary>
    /// Base interface for objects than can be rendered to the screen.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// Render this object to screen, using given render parameters.
        /// </summary>
        /// <param name="rp">Rendering parameters to use</param>
        void Render(RenderParams rp);
    }
}