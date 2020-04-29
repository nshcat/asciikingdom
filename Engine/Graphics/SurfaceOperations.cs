using System;
using System.Runtime.CompilerServices;
using System.Text;
using Engine.Core;
using Engine.Graphics;

namespace Engine.Graphics
{
    /// <summary>
    /// Static class containing extension methods that allow more complex modifications
    /// of ASCII surfaces
    /// </summary>
    public static class SurfaceOperations
    {
        /// <summary>
        /// Draw string to surface at given position. Will automatically cut off the string if it would exceed surface
        /// bounds.
        /// </summary>
        /// <param name="surface">Target surface</param>
        /// <param name="position">Start position</param>
        /// <param name="text">Text to draw</param>
        /// <param name="front">Foreground color</param>
        /// <param name="back">Background color</param>
        public static void DrawString(
            this Surface surface,
            Position position,
            string text,
            Color front,
            Color back)
        {
            if(!position.IsInBounds(surface.Bounds))
                throw new ArgumentException("DrawString: Start position out of bounds");
            
            // Retrieve ASCII bytes
            var characters = Encoding.ASCII.GetBytes(text);
            
            // Calculate maximum length of string we can draw
            var end = Math.Min((surface.Dimensions.Width - 1) - position.X, characters.Length - 1);

            for (var ix = 0; ix <= end; ++ix)
            {
                var tile = new Tile(characters[ix], front, back);
                var characterPosition = new Position(position.X + ix, position.Y);
                surface.SetTile(characterPosition, tile);
            }
        }
    }
}