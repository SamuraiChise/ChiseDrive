using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChiseDrive.Graphics
{
    public class Line : DrawablePrimitive3D
    {
        Vector3 origin;
        Vector3 direction;
        Vector2 size;
        Color color;

        VertexPositionColor[] vertices;

        static Effect effect;
        static VertexDeclaration declaration;
        static public void Initialize(XnaReference reference)
        {
            effect = reference.Content.Load<Effect>("Effects/SimpleEffect");
            declaration = new VertexDeclaration(reference.Device, VertexPositionColor.VertexElements);
        }

        public Vector3 Origin
        {
            get
            {
                return origin;
            }
            set
            {
                origin = value;
            }
        }
        public Vector3 Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
            }
        }
        public float Length
        {
            get
            {
                return size.X;
            }
            set
            {
                size.X = value;
            }
        }
        public float Thickness
        {
            get
            {
                return size.Y;
            }
            set
            {
                size.Y = value;
            }
        }
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        public Line(Vector3 origin, Vector3 direction, Vector2 size, Color color)
        {
            this.origin = origin;
            this.direction = direction;
            this.size = size;
            this.color = color;
            this.vertices = new VertexPositionColor[4];
        }

        void FillVertices(Cameras.Camera camera)
        {
            Vector3 centerpoint = origin;
            centerpoint += (direction * Length * 0.5f);

            Vector3 tocamera = camera.Position - centerpoint;
            Helper.Normalize(ref tocamera);
            Vector3 width = Vector3.Cross(direction, tocamera);
            width *= Thickness;
            Vector3 length = direction * Length;

            vertices[0].Position = origin + width;
            vertices[1].Position = origin - width;
            vertices[2].Position = origin + width + length;
            vertices[3].Position = origin - width + length;

            vertices[0].Color = color;
            vertices[1].Color = color;
            vertices[2].Color = color;
            vertices[3].Color = color;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, ChiseDrive.Cameras.Camera camera)
        {
            FillVertices(camera);

            effect.CurrentTechnique = effect.Techniques[0];

            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(camera.View);
            effect.Parameters["Projection"].SetValue(camera.Projection);

            device.VertexDeclaration = declaration;

            effect.Begin();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip,
                    vertices, 0, 2);
                pass.End();
            }

            effect.End();
        }
    }
}