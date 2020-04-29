using System;
using System.Collections.Generic;
using OpenToolkit.Mathematics;

namespace Engine.Rendering
{
    /// <summary>
    /// A class holding important rendering parameters, such as the view and projection matrices.
    /// </summary>
    public class RenderParams
    {
        /// <summary>
        /// The view matrix that should be used for this rendering job
        /// </summary>
        public Matrix4 View { get; protected set; }
        
        /// <summary>
        /// The projection matrix that should be used for this rendering job
        /// </summary>
        public Matrix4 Projection { get; protected set; }

        /// <summary>
        /// Create a new set of rendering parameters using given view and projection matrices.
        /// The model matrix is initialized to the identity matrix.
        /// </summary>
        /// <param name="view">View matrix</param>
        /// <param name="projection">Projection matrix</param>
        public RenderParams(Matrix4 view, Matrix4 projection)
        {
            this.View = view;
            this.Projection = projection;
        }
    }
}