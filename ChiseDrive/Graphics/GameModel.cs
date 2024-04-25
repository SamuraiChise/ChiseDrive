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
    /// <summary>
    /// Builds an interface around the Xna.Framework.Graphics.Model
    /// </summary>
    public class GameModel : DrawableMesh
    {
        Model model;

        Matrix[] bonetransforms = null;
        float[] bonelengths = null;
        Matrix roottransform = Matrix.Identity;
        Matrix worldtransforms = Matrix.Identity;
        Matrix transforms = Matrix.Identity;
        float scale = 1f;

        AnimationPlayer animationPlayer;
        SkinningData skinningData;

        public Model Model
        {
            get
            {
                return model;
            }
        }
        public override AnimationPlayer AnimationPlayer
        {
            get 
            {
                if (animationPlayer == null) throw new Exception("This model does not have an animationPlayer.  Should this model be built with the SkinnedAnimation processor?");
                return animationPlayer; 
            }
        }

        public override Matrix[] SkinningBones
        {
            get
            {
                if (animationPlayer != null)
                {
                    return animationPlayer.GetSkinTransforms();
                }
                return null;
            }
        }
        public Matrix BaseMatrix
        {
            set
            {
                roottransform = value;
                RebuildTransforms();
            }
        }
        public Matrix WorldTransform
        {
            set
            {
                worldtransforms = value;
                RebuildTransforms();
            }
        }
        public Matrix Transform
        {
            get
            {
                return transforms;
            }
        }
        public override Matrix RotationPosition
        {
            get
            {
                return transforms;
            }
            set
            {
                WorldTransform = value;
            }
        }
        public float Scale
        {
            set
            {
                scale = value;
                RebuildTransforms();
                RebuildBounding();
            }
            get
            {
                return scale;
            }
        }
        public float BoundingHack { get; set; }

        #region IBounding
        BoundingSphere modelBounding;
        BoundingSphere worldBounding;

        public override Nullable<Vector4> Intersects(Ray test, float length)
        {
            if (ChiseDriveGame.Force2D)
            {
                worldBounding.Center.Z = 0f;
            }
            float? value = worldBounding.Intersects(test);

            if (value == null || value > length) return null;

            // Look up our custom collision data from the Tag property of the model.
            Dictionary<string, object> tagData = (Dictionary<string, object>)model.Tag;
            
            if (tagData != null && tagData.ContainsKey("Vertices"))
            {
                Matrix inverseTransform = Matrix.Invert(transforms);
                test.Position = Vector3.Transform(test.Position, inverseTransform);
                test.Direction = Vector3.TransformNormal(test.Direction, inverseTransform);

                // The bounding sphere test passed, so we need to do a full
                // triangle picking test.

                // Keep track of the closest triangle we found so far,
                // so we can always return the closest one.
                float? closestIntersection = null;

                // Loop over the vertex data, 3 at a time (3 vertices = 1 triangle).
                Vector3[] vertices = (Vector3[])tagData["Vertices"];

                for (int i = 0; i < vertices.Length; i += 3)
                {
                    // Perform a ray to triangle intersection test.
                    float? intersection;

                    Helper.RayIntersectsTriangle(ref test,
                                              ref vertices[i],
                                              ref vertices[i + 1],
                                              ref vertices[i + 2],
                                              out intersection);

                    // Does the ray intersect this triangle?
                    if (intersection != null)
                    {
                        // If so, is it closer than any other previous triangle?
                        if ((closestIntersection == null) ||
                            (intersection < closestIntersection))
                        {
                            // Store the distance to this triangle.
                            closestIntersection = intersection;
                        }
                    }
                }

                if (closestIntersection != null)
                {
                    // Return to world coordinates
                    test.Position = Vector3.Transform(test.Position, transforms);
                    test.Direction = Vector3.TransformNormal(test.Direction, transforms);

                    Vector3 rayintersect = test.Position + test.Direction * (float)closestIntersection;
                    Vector4 retvalue = new Vector4(rayintersect, (float)closestIntersection);
                    return retvalue;
                }
                return null;
            }
            
            // No information for triangle collision
            Vector3 intersect = test.Position + test.Direction * (float)value;
            return new Vector4(intersect, (float)value);

        }

        public override Nullable<Vector4> Intersects(BoundingBox test)
        {
            if (!worldBounding.Intersects(test)) return null;

            Vector3 testcenter = test.Min + (test.Max - test.Min) / 2f;
            Vector3 intersect = worldBounding.Center + (testcenter - worldBounding.Center) / 2f;
            float distance = Vector3.Distance(intersect, worldBounding.Center);
            return new Vector4(intersect, distance);
        }

        public override Nullable<Vector4> Intersects(BoundingSphere test)
        {
            if (ChiseDriveGame.Force2D)
            {
                worldBounding.Center.Z = 0f;
            }
            bool sphereIntersects = worldBounding.Intersects(test);

            if (sphereIntersects == false) return null;

            if (model.Tag != null && model.Tag is Dictionary<string, object>)
            {
                // Look up our custom collision data from the Tag property of the model.
                Dictionary<string, object> tagData = (Dictionary<string, object>)model.Tag;

                if (tagData != null && tagData.ContainsKey("Vertices"))
                {
                    Matrix inverseTransform = Matrix.Invert(transforms);
                    test.Center = Vector3.Transform(test.Center, inverseTransform);
                    test.Radius /= BoundingHack;

                    // The bounding sphere test passed, so we need to do a full
                    // triangle picking test.

                    // Keep track of the closest triangle we found so far,
                    // so we can always return the closest one.
                    float? closestIntersection = null;

                    // Loop over the vertex data, 3 at a time (3 vertices = 1 triangle).
                    Vector3[] vertices = (Vector3[])tagData["Vertices"];

                    Vector3 center = Vector3.Zero;
                    Vector3 center1 = Vector3.Zero;

                    for (int i = 0; i < vertices.Length; i += 3)
                    {
                        // Perform a ray to triangle intersection test.
                        float? intersection;

                        Helper.SphereIntersectsTriangle(ref test,
                                                  ref vertices[i],
                                                  ref vertices[i + 1],
                                                  ref vertices[i + 2],
                                                  out intersection);

                        // Does the ray intersect this triangle?
                        if (intersection != null)
                        {
                            // If so, is it closer than any other previous triangle?
                            if ((closestIntersection == null) ||
                                (intersection < closestIntersection))
                            {
                                // Store the distance to this triangle.
                                closestIntersection = intersection;

                                Helper.TriangleCenter(ref vertices[i], ref vertices[i + 1], ref vertices[i + 2], out center);
                                center1 = center;
                                Vector3.Transform(ref center1, ref transforms, out center);
                            }
                        }
                    }

                    if (closestIntersection != null)
                    {
                        // Return to world coordinates
                        test.Center = Vector3.Transform(test.Center, transforms);
                        test.Radius *= scale;

                        Vector3 tosphere = test.Center - center;
                        tosphere.Normalize();

                        Vector3 impact = tosphere * (float)closestIntersection;
                        float distance = Vector3.Distance(impact, test.Center);
                        return new Vector4(impact, distance);
                    }
                    return null;
                }
            }

            Vector3 intersect = worldBounding.Center + (test.Center - worldBounding.Center) / 2f;
            float dist = Vector3.Distance(intersect, worldBounding.Center);
            return new Vector4(intersect, dist);

        }

        public override Nullable<Vector4> Intersects(IBounding test)
        {
            return test.Intersects(worldBounding);
        }

        public override BoundingBox BoundingBox
        {
            get { return BoundingBox.CreateFromSphere(BoundingSphere); }
        }
        public override BoundingSphere BoundingSphere
        {
            get { return worldBounding; }
        }
        #endregion

        public GameModel(Model model, float scale)
        {
            this.model = model;
            this.scale = scale;

            BoundingHack = 1f;

            // Skin this model (by putting bones in it?)
            bonetransforms = new Matrix[model.Bones.Count];
            bonelengths = new float[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bonetransforms);
            for (int i = 0; i < model.Bones.Count; i++)
            {
                if (model.Bones[i].Children.Count == 0)
                {
                    // Impossible to calculate the length
                    // of a bone without children.
                    bonelengths[i] = 0f;
                }
                else
                {
                    Matrix transform = model.Bones[i].Transform;
                    Matrix child = model.Bones[i].Children[0].Transform;

                    bonelengths[i] = Vector3.Distance(transform.Translation,
                        child.Translation);
                }
            }

            skinningData = model.Tag as SkinnedModel.SkinningData;
            if (skinningData != null) animationPlayer = new AnimationPlayer(skinningData);

            RebuildTransforms();
            RebuildBounding();
        }
        public GameModel(GameModel copy)
        {
            this.model = copy.model;
            this.scale = copy.scale;

            BoundingHack = 1f;

            // Skin this model (by putting bones in it?)
            bonetransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bonetransforms);

            bonelengths = new float[model.Bones.Count];
            for (int i = 0; i < model.Bones.Count; i++)
            {
                bonelengths[i] = copy.bonelengths[i];
            }

            skinningData = model.Tag as SkinnedModel.SkinningData;
            if (skinningData != null) animationPlayer = new AnimationPlayer(skinningData);

            RebuildTransforms();
            RebuildBounding();
        }

        public override DrawableMesh Clone()
        {
            return new GameModel(this);
        }
        public void Dispose()
        {
            model = null;
        }

        void RebuildBounding()
        {
            modelBounding = new BoundingSphere();

            foreach (ModelMesh mesh in model.Meshes)
            {
                Matrix boundtransform = bonetransforms[mesh.ParentBone.Index] * roottransform * Matrix.CreateScale(scale);
                BoundingSphere sphere = mesh.BoundingSphere.Transform(boundtransform);
                modelBounding = BoundingSphere.CreateMerged(modelBounding, sphere);
            }
        }
        void RebuildTransforms()
        {
            transforms = roottransform * Matrix.CreateScale(scale) * worldtransforms;
            worldBounding.Center = Vector3.Transform(modelBounding.Center, worldtransforms);
            worldBounding.Radius = modelBounding.Radius * BoundingHack;
        }
        void BuildTransform(Vector3 position, Quaternion rotation)
        {
            worldtransforms = Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);
            RebuildTransforms();
        }
        void BuildTransform(Matrix positionrotation)
        {
            worldtransforms = positionrotation;
            RebuildTransforms();
        }
        protected Matrix BoneWorldTransform(int index)
        {
            return bonetransforms[index] * transforms;
        }

        // From DrawableMesh **************************************************
        public override Matrix BoneTransform(string name)
        {
            if (animationPlayer != null)
            {
                return AnimationPlayer.GetBoneTransform(name);
            }
            else
            {
                return bonetransforms[model.Bones[name].Index];
            }
        }
        public override float BoneLength(string name)
        {
            return bonelengths[model.Bones[name].Index];
        }
        public override List<String> GetBoneNames()
        {
            List<String> names = new List<String>();
            
            for (int b = 0; b < model.Bones.Count; b++)
            {
                names.Add(model.Bones[b].Name);
            }

            return names;
        }

        public override void Update(Time elapsed)
        {
            TimeSpan time = new TimeSpan(elapsed.Ticks);
            if (animationPlayer != null)
            {
                animationPlayer.Update(time, true, Matrix.CreateTranslation(Vector3.Zero));
            }            
        }

        public override void Draw(GraphicsDevice device, Camera camera)
        {
            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in model.Meshes)
            {
                effect.Parameters["World"].SetValue(BoneWorldTransform(mesh.ParentBone.Index));
                effect.CommitChanges();
                device.Indices = mesh.IndexBuffer;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    device.Vertices[0].SetSource(
                        mesh.VertexBuffer, meshPart.StreamOffset,
                        meshPart.VertexStride);

                    device.VertexDeclaration = meshPart.VertexDeclaration;

                    device.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        meshPart.BaseVertex,
                        0, meshPart.NumVertices, meshPart.StartIndex,
                        meshPart.PrimitiveCount);

                    #region Polygon Count
#if Debug
                    PolygonsDrawn += meshPart.PrimitiveCount;
#endif
                    #endregion
                }
            }
        }

#if Debug
        public override int PolygonCount
        {
            get
            {
                int count = 0;
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        count += part.PrimitiveCount;
                    }
                }
                return count;
            }
        }
#endif
    }
}
