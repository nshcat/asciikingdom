using Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Ui.Toolkit
{
    /// <summary>
    /// Class encapsulating current layouting context for the GUI
    /// </summary>
    public class LayoutContext
    {
        #region Properties
        /// <summary>
        /// The top left corner of the accesible area for the GUI.
        /// This is e.g. used for windows and affects new lines, indentation etc.
        /// </summary>
        public Position Origin { get; set; }
            = new Position(0, 0);

        /// <summary>
        /// The current position in the layout.
        /// </summary>
        public Position CurrentPosition { get; set; }
            = new Position(0, 0);

        /// <summary>
        /// The current indent level. This gets multiplied with the indent depth.
        /// </summary>
        public int IndentLevel { get; set; }
            = 0;

        /// <summary>
        /// Depth per indent depth, per cell.
        /// </summary>
        public int IndentDepth { get; set; }
            = 2;

        /// <summary>
        /// Whether the next new line caused by a widget is supposed
        /// to be ignored.
        /// </summary>
        public bool InhibitNewline { get; set; }
            = false;
        #endregion

        #region Layout Modification Methods
        /// <summary>
        /// Move to the next line, used by widget methods and includes extra logic
        /// for stuff like new line inhibition
        /// </summary>
        public void NextLineWidget()
        {
            // Inhibit new line if requested by user
            if(this.InhibitNewline)
            {
                this.InhibitNewline = false;
                return;
            }

            this.NextLine();
        }

        /// <summary>
        /// Move to the next line, honouring indentation.
        /// </summary>
        public void NextLine()
        {
            this.CurrentPosition = new Position(
                this.Origin.X + this.IndentLevel * this.IndentDepth,
                this.CurrentPosition.Y + 1);
        }

        /// <summary>
        /// Increase the current indentation level. Adjusts the 
        /// current position if its at the beginning of a new line.
        /// </summary>
        public void IncreaseIndent(int value)
        {
            if (this.CurrentPosition.X == this.Origin.X + (this.IndentLevel * this.IndentDepth))
            {
                this.CurrentPosition = new Position(
                    this.Origin.X + ((this.IndentLevel + value) * this.IndentDepth),
                    this.CurrentPosition.Y
                    );
            }

            this.IndentLevel += value;
        }

        /// <summary>
        /// Increase the current indentation level. Adjusts the 
        /// current position if its at the beginning of a new line.
        /// </summary>
        public void DecreaseIndent(int value)
        {
            if (this.CurrentPosition.X == this.Origin.X + (this.IndentLevel * this.IndentDepth))
            {
                this.CurrentPosition = new Position(
                    this.Origin.X + ((this.IndentLevel - value) * this.IndentDepth),
                    this.CurrentPosition.Y
                    );
            }

            this.IndentLevel -= value;
        }
        #endregion
    }
}
