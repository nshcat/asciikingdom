using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;

namespace Game.Ui.Toolkit
{
    /// <summary>
    /// Rendering command that draws a single tile
    /// </summary>
    public class DrawTileCommand
        : IRenderingCommand
    {
        /// <summary>
        /// The glyph to draw
        /// </summary>
        protected int _glyph;

        public DrawTileCommand(int glyph)
        {
            this._glyph = glyph;
        }

        public void ApplyTo(RenderingContext context)
        {
            var (front, back) = context.GetRenderingColors();

            context.Target.SetTile(context.GetDrawingPosition(), new Tile(this._glyph, front, back));
        }
    }
}
