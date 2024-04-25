using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChiseDrive.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Graphics.SkinnedModel;

namespace ChiseDrive.Graphics
{
    public abstract class DrawableMesh : IBounding
    {
        #region IBounding
        public abstract Nullable<Vector4> Intersects(Ray test, float length);
        public abstract Nullable<Vector4> Intersects(BoundingBox test);
        public abstract Nullable<Vector4> Intersects(BoundingSphere test);
        public abstract Nullable<Vector4> Intersects(IBounding test);

        public abstract BoundingBox BoundingBox { get; }
        public abstract BoundingSphere BoundingSphere { get; }
        #endregion

        protected Effect effect;
        public Effect Effect { set { effect = value; } }

        public abstract Matrix RotationPosition { get; set; }
        public abstract AnimationPlayer AnimationPlayer { get; }
        public abstract Matrix[] SkinningBones { get; }
        public abstract DrawableMesh Clone();

        /// <summary>
        /// Returns an offset matrix for a bone by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract Matrix BoneTransform(string name);
        public abstract float BoneLength(string name);

        /// <summary>
        /// Returns a list of all the bones by name for this object.
        /// </summary>
        /// <returns></returns>
        public abstract List<String> GetBoneNames();

        /// <summary>
        /// Updates any animations or other effects.
        /// </summary>
        /// <param name="elapsed"></param>
        public abstract void Update(Time elapsed);

        /// <summary>
        /// Draws a model with the current graphics settings.
        /// </summary>
        /// <param name="camera">The camera to draw.</param>
        public abstract void Draw(GraphicsDevice device, Camera camera);

        #region Polygon Count
#if Debug
        public abstract int PolygonCount { get; }
        protected static int PolygonsDrawn = 0;
        public static int GetPolygonCount()
        {
            int retval = PolygonsDrawn;
            PolygonsDrawn = 0;
            return retval;
        }
#endif
        #endregion
    }
}