using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Cameras;

namespace ChiseDrive.Graphics
{
    public class CustomMultiTextureMesh : DrawableMesh
    {
        #region IBounding
        public override Nullable<Vector4> Intersects(Ray test, float length) { return null; }
        public override Nullable<Vector4> Intersects(BoundingBox test) { return null; }
        public override Nullable<Vector4> Intersects(BoundingSphere test) { return null; }
        public override Nullable<Vector4> Intersects(IBounding test) { return null; }
        public override BoundingBox BoundingBox
        {
            get { throw new NotImplementedException(); }
        }
        public override BoundingSphere BoundingSphere
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region Drawable
        public override Matrix RotationPosition
        {
            get
            {
                return Matrix.CreateTranslation(new Vector3(0f, 0f, -800f));
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public override Matrix[] SkinningBones
        {
            get { return null; }
        }
        public override ChiseDrive.Graphics.SkinnedModel.AnimationPlayer AnimationPlayer
        {
            get { throw new NotImplementedException(); }
        }
        /// <summary>
        /// Returns an offset matrix for a bone by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override Matrix BoneTransform(string name)
        {
            return Matrix.Identity;
        }
        public override float BoneLength(string name)
        {
            return 0f;
        }
        /// <summary>
        /// Returns a list of all the bones by name for this object.
        /// </summary>
        /// <returns>Null</returns>
        public override List<String> GetBoneNames()
        {
            return null;
        }
        #endregion

        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        VertexDeclaration declaration;

        const int TextureCount = 4;

        Material[] Materials = new Material[TextureCount];
        public void SetMaterial(Material mat, int index)
        {
            Materials[index] = mat;

            this.effect.Parameters["Texture" + index + "d"].SetValue(mat.Diffuse);
            this.effect.Parameters["Texture" + index + "s"].SetValue(mat.Specular);
            this.effect.Parameters["Texture" + index + "n"].SetValue(mat.Normal);

            this.effect.CommitChanges();
        }

        public CustomMultiTextureMesh(GraphicsDevice device, VertexPositionNormalColorTextureMulti[] vertices, int[] indices, VertexDeclaration declaration)
        {
            this.declaration = declaration;

            vertexBuffer = new VertexBuffer(device, vertices.Length * VertexPositionNormalColorTextureMulti.SizeInBytes, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        public override DrawableMesh Clone()
        {
            throw new NotImplementedException();
        }

        public override void Draw(GraphicsDevice device, Camera camera)
        {
            Matrix worldMatrix = Matrix.Identity;
            effect.Parameters["World"].SetValue(worldMatrix);
            device.Indices = indexBuffer;

            device.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionNormalColorTextureMulti.SizeInBytes);
            device.Indices = indexBuffer;
            device.VertexDeclaration = declaration;

            int vertexCount = vertexBuffer.SizeInBytes / VertexPositionNormalColorTextureMulti.SizeInBytes;
            int triangleCount = indexBuffer.SizeInBytes / sizeof(int) / 3;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, triangleCount);

            #region Polygon Count
#if Debug
            PolygonsDrawn += triangleCount;
#endif
            #endregion
        }

        public override void Update(Time elapsed)
        {   
        }

#if Debug
        public override int PolygonCount
        {
            get 
            {
                return vertexBuffer.SizeInBytes / VertexPositionNormalColorTextureMulti.SizeInBytes;
            }
        }
#endif
    }
}