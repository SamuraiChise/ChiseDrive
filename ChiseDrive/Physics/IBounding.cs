using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ChiseDrive.Physics
{
    public interface IBounding
    {
        /// <summary>
        /// Test vs Ray
        /// </summary>
        /// <param name="test">The ray to test.</param>
        /// <returns>Point of intersection, W value holds distance along ray till intersect.</returns>
        Nullable<Vector4> Intersects(Ray test);

        /// <summary>
        /// Test vs Box
        /// </summary>
        /// <param name="test">The box to test.</param>
        /// <returns>Point of intersection.</returns>
        Nullable<Vector4> Intersects(BoundingBox test);
        Nullable<Vector4> Intersects(BoundingSphere test);
        Nullable<Vector4> Intersects(IBounding test);
    }
}