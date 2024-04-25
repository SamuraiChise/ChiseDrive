using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Cameras;

namespace ChiseDrive.Graphics
{
    /// <summary>
    /// A custom 
    /// </summary>
    public class CustomMesh : DrawableMesh
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

        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        VertexDeclaration declaration;

        public override Matrix RotationPosition
        {
            get
            {
                throw new NotImplementedException();
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

        public CustomMesh(GraphicsDevice device, VertexPositionNormalColorTexture[] vertices, int[] indices, VertexDeclaration declaration)
        {
            this.declaration = declaration;

            VertexPositionNormalColorTexture[] normalized = CalculateNormals(vertices, indices);
            //VertexMultitextured[] normalized = CalculateNormals(vertices, indices);

            vertexBuffer = new VertexBuffer(device, normalized.Length * VertexMultitextured.SizeInBytes, BufferUsage.WriteOnly);
            vertexBuffer.SetData(normalized);

            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        public override DrawableMesh Clone()
        {
            throw new NotImplementedException();
        }

        VertexPositionNormalColorTexture[] CalculateNormals(
            VertexPositionNormalColorTexture[] vertices, int[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = Vector3.Zero;

            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();

            return vertices;
        }

        /*
        VertexMultitextured[] CalculateNormals(VertexMultitextured[] vertices, int[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();

            return vertices;
        }*/

        public override void Update(Time elapsed)
        {
            
        }

        /// <summary>
        /// Draws a model with the current graphics settings.
        /// </summary>
        /// <param name="camera">The camera to draw.</param>
        public override void Draw(GraphicsDevice device, Camera camera)
        {
            Matrix worldMatrix = Matrix.Identity;
            effect.Parameters["World"].SetValue(worldMatrix);
            device.Indices = indexBuffer;

            device.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionNormalColorTexture.SizeInBytes);
            device.Indices = indexBuffer;
            device.VertexDeclaration = declaration;

            int vertexCount = vertexBuffer.SizeInBytes / VertexPositionNormalColorTexture.SizeInBytes;
            int triangleCount = indexBuffer.SizeInBytes / sizeof(int) / 3;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, triangleCount);

            #region Polygon Count
#if Debug
            PolygonsDrawn += triangleCount;
#endif
            #endregion
        }

#if Debug
        public override int PolygonCount
        {
            get { return vertexBuffer.SizeInBytes / VertexPositionNormalColorTexture.SizeInBytes; }
        }
#endif
    }
}