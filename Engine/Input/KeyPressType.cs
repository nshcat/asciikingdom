namespace Engine.Input
{
    /// <summary>
    /// Enumeration describing all recognized types of key interactions
    /// </summary>
    public enum KeyPressType
    {
        /// <summary>
        /// Key is currently down
        /// </summary>
        Down,
        
        /// <summary>
        /// Key was just released
        /// </summary>
        Released,
        
        /// <summary>
        /// Key was just pressed
        /// </summary>
        Pressed
    }
}