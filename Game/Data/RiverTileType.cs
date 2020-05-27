namespace Game.Data
{
    /// <summary>
    /// Represents the different states a river terrain tile can be in.
    /// </summary>
    public enum RiverTileType
    {
        None,
        Source,
        Horizontal,
        Vertical,
        Cross,
        NorthWest,
        NorthEast,
        SouthWest,
        SouthEast,
        HorizontalNorth,
        HorizontalSouth,
        VerticalEast,
        VerticalWest
    }
}