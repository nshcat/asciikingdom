using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Engine.Core;
using Engine.Graphics;
using Rectangle = Engine.Core.Rectangle;

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
            // Retrieve ASCII bytes
            var characters = Encoding.ASCII.GetBytes(text);

            for (var ix = 0; ix < characters.Length; ++ix)
            {
                var characterPosition = new Position(position.X + ix, position.Y);
                
                var tile = new Tile(characters[ix], front, back);
                surface.SetTile(characterPosition, tile);
            }
        }

        /// <summary>
        /// Draw string to surface centered at given position. Will automatically cut off the string if it would exceed
        /// surface bounds.
        /// </summary>
        /// <param name="surface">Target surface</param>
        /// <param name="position">Center position</param>
        /// <param name="text">Text to draw</param>
        /// <param name="front">Foreground color</param>
        /// <param name="back">Background color</param>
        public static void DrawStringCentered(
            this Surface surface,
            Position position,
            string text,
            Color front,
            Color back)
        {
            var startPosition = new Position(position.X - text.Length/2, position.Y);
            surface.DrawString(startPosition, text, front, back);
        }

        /// <summary>
        /// Draw a single-stroke border based on given rectangle.
        /// </summary>
        /// <param name="surface">Target surface</param>
        /// <param name="bounds">Border bounds</param>
        /// <param name="front">Foreground color to use</param>
        /// <param name="back">Background color to use</param>
        public static void DrawBorder(
            this Surface surface,
            Rectangle bounds,
            Color front,
            Color back
        )
        {
            var topLeft = bounds.TopLeft;
            var topRight = bounds.TopRight;
            var bottomLeft = bounds.BottomLeft;
            var bottomRight = bounds.BottomRight;
            
            
            var horizontalTile = new Tile((int)BoxCharacters.Horizontal, front, back);
            for (var ix = topLeft.X; ix <= topRight.X; ++ix)
            {
                surface.SetTile(new Position(ix, topLeft.Y), horizontalTile);
                surface.SetTile(new Position(ix, bottomRight.Y), horizontalTile);
            }
            
            var verticalTile = new Tile((int)BoxCharacters.Vertical, front, back);
            for (var iy = topLeft.Y; iy <= bottomLeft.Y; ++iy)
            {
                surface.SetTile(new Position(topLeft.X, iy), verticalTile);
                surface.SetTile(new Position(topRight.X, iy), verticalTile);
            }
            
            surface.SetTile(topLeft, new Tile((int)BoxCharacters.CornerTopLeft, front, back));
            surface.SetTile(topRight, new Tile((int)BoxCharacters.CornerTopRight, front, back));
            surface.SetTile(bottomLeft, new Tile((int)BoxCharacters.CornerBottomLeft, front, back));
            surface.SetTile(bottomRight, new Tile((int)BoxCharacters.CornerBottomRight, front, back));
        }

        /// <summary>
        /// Fill given area on surface with a tile
        /// </summary>
        public static void FillArea(
            this Surface surface,
            Rectangle area,
            Tile tile
        )
        {
            for (var ix = area.TopLeft.X; ix <= area.BottomRight.X; ++ix)
            {
                for (var iy = area.TopLeft.Y; iy <= area.BottomRight.Y; ++iy)
                {
                    surface.SetTile(new Position(ix, iy), tile);
                }
            }
        }

        /// <summary>
        /// Draw a key binding hint, and return X coordinate of last character.
        /// </summary>
        /// <param name="surface">Surface to draw on</param>
        /// <param name="position">Position to draw the key binding hint</param>
        /// <param name="key">Key symbol</param>
        /// <param name="text">Descriptive text</param>
        /// <param name="keyFront">Foreground color for key symbol</param>
        /// <param name="textFront">Foreground color for description</param>
        /// <param name="back">Background color</param>
        /// <returns>X coordinate of last printed character</returns>
        public static int DrawKeybinding(this Surface surface, Position position, string key, string text,
            Color keyFront, Color textFront, Color back)
        {
            surface.DrawString(position, key, keyFront, back);
            surface.DrawString(new Position(position.X + key.Length, position.Y),
                $": {text}", textFront, back);

            return position.X + key.Length + 1 + text.Length;
        }
        
        /// <summary>
        /// Draw a window consisting of a border and a title.
        /// </summary>
        /// <param name="surface">Target surface</param>
        /// <param name="bounds">Position and dimensions of the window</param>
        /// <param name="title">Window title</param>
        /// <param name="borderFront">Border foreground color</param>
        /// <param name="borderBack">Border background color</param>
        /// <param name="titleFront">Tittle foreground color</param>
        /// <param name="fillBack">The color used to fill the inner parts of the window</param>
        public static void DrawWindow(
            this Surface surface,
            Rectangle bounds,
            string title,
            Color borderFront,
            Color borderBack,
            Color titleFront,
            Color fillBack
        )
        {
            if(string.IsNullOrEmpty(title))
                throw new ArgumentException("DrawWindow: Title can't be empty");

            var maxLength = bounds.Size.Width - 4;
            var truncatedTitle = (title.Length <= maxLength) ? title : title.Substring(0, maxLength);
            
            surface.FillArea(bounds, new Tile(0, DefaultColors.Black, fillBack));
            surface.DrawBorder(bounds, borderFront, borderBack);
            var centerX = bounds.TopLeft.X + (bounds.Size.Width / 2);
            surface.DrawStringCentered(new Position(centerX, bounds.TopLeft.Y), truncatedTitle, titleFront, borderBack);

            var beforeTitleX = centerX - ((truncatedTitle.Length / 2) + 1);
            var afterTitleX = centerX + (truncatedTitle.Length / 2);
            
            surface.SetTile(new Position(beforeTitleX, bounds.TopLeft.Y), new Tile((int)BoxCharacters.VerticalLeft, borderFront, borderBack));
            surface.SetTile(new Position(afterTitleX, bounds.TopLeft.Y), new Tile((int)BoxCharacters.VerticalRight, borderFront, borderBack));

            for (var ix = 0; ix < bounds.Size.Width; ++ix)
            {
                var position = new Position(bounds.TopLeft.X + 1 + ix, bounds.BottomRight.Y + 1);
                surface.SetUiShadow(position, true);
            }
            
            for (var iy = 0; iy < bounds.Size.Height; ++iy)
            {
                var position = new Position(bounds.TopRight.X + 1, bounds.TopLeft.Y + 1 + iy);
                surface.SetUiShadow(position, true);
            }
        }

        /// <summary>
        /// Draw a rectangle of given glyph with given colors
        /// </summary>
        public static void DrawRectangle(
            this Surface surface,
            Rectangle bounds,
            int glyph,
            Color front,
            Color back
        )
        {
            var tile = new Tile(glyph, front, back);
            var topLeft = bounds.TopLeft;
            var topRight = bounds.TopRight;
            var bottomLeft = bounds.BottomLeft;
            var bottomRight = bounds.BottomRight;
            
            for (var ix = topLeft.X; ix <= topRight.X; ++ix)
            {
                surface.SetTile(new Position(ix, topLeft.Y), tile);
                surface.SetTile(new Position(ix, bottomRight.Y), tile);
            }
            
            for (var iy = topLeft.Y; iy <= bottomLeft.Y; ++iy)
            {
                surface.SetTile(new Position(topLeft.X, iy), tile);
                surface.SetTile(new Position(topRight.X, iy), tile);
            }
        }

        /// <summary>
        /// Draw a progress bar in given bounds.
        /// </summary>
        /// <param name="surface">Target surface</param>
        /// <param name="bounds">Bounds to fill with progress bar</param>
        /// <param name="value">Current progress bar value, in [0, 1]</param>
        /// <param name="front">Foreground color to use</param>
        /// <param name="back">Background color to use</param>
        public static void DrawProgressBar(
            this Surface surface,
            Rectangle bounds,
            double value,
            Color front,
            Color back
        )
        {
            // Clamp progress value to [0.0, 1.0] to avoid issues
            value = Math.Clamp(value, 0.0, 1.0);
            
            var filledSegment = new Tile(219, front, back);
            var halfFilledSegment = new Tile(221, front, back);
            var emptySegment = new Tile(219, back, back);

            var progress = value * bounds.Size.Width;
            
            // Draw filled segments
            var filledSegments = (int) progress;
            for (var ix = 0; ix < filledSegments; ++ix)
            {
                for (var iy = 0; iy < bounds.Size.Height; ++iy)
                {
                    surface.SetTile(new Position(bounds.TopLeft.X + ix, bounds.TopLeft.Y + iy), filledSegment);
                }
            }
            
            // Draw potentially half-filled segment
            var fraction = progress - Math.Truncate(progress);
            var drawHalfSegment = (fraction >= 0.5);

            if (drawHalfSegment)
            {
                for (var iy = 0; iy < bounds.Size.Height; ++iy)
                {
                    surface.SetTile(new Position(bounds.TopLeft.X + filledSegments, bounds.TopLeft.Y + iy), halfFilledSegment);
                }
            }

            // Draw empty segments
            for (var ix = (drawHalfSegment ? 1 : 0) + filledSegments; ix < bounds.Size.Width; ++ix)
            {
                for (var iy = 0; iy < bounds.Size.Height; ++iy)
                {
                    surface.SetTile(new Position(bounds.TopLeft.X + ix, bounds.TopLeft.Y + iy), emptySegment);
                }
            }
        }
    }
}