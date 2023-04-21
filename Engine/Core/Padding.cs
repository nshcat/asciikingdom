using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Core
{
    /// <summary>
    /// Class that describes 
    /// </summary>
    public struct Padding
    {
        /// <summary>
        /// Padding on the left
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// Padding on the right
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        /// Padding on the top
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// Padding on the bottom
        /// </summary>
        public int Bottom { get; set; }

        /// <summary>
        /// Create a new padding instance
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        public Padding(int left = 0, int right = 0, int top = 0, int bottom = 0)
        {
            this.Left = left;
            this.Right = right;
            this.Top = top;
            this.Bottom = bottom;
        }
    }
}
