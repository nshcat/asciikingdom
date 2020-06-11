using Engine.Graphics;

namespace Game.Ui
{
    /// <summary>
    /// Colors commonly used in the game ui
    /// </summary>
    public static class UiColors
    {
        public static Color BorderBack => new Color(77, 77, 77);
        public static Color BorderFront => new Color(158, 158, 158);
        public static Color BorderTitle => new Color(222, 222, 222);
        public static Color ActiveText => new Color(222, 222, 222);
        public static Color InactiveText => new Color(158, 158, 158);
        public static Color Keybinding => Color.FromHex("#38EC00");
    }
}