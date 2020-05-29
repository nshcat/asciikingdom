namespace Game.Data
{
    /// <summary>
    /// Base interface for all type classes in the game. Provides basic identification support.
    /// </summary>
    public interface ITypeClass
    {
        /// <summary>
        /// A unique identifier used to differentiate between type classes
        /// </summary>
        public string Identifier { get; set; }
    }
}