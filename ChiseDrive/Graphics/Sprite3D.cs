using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChiseDrive.Graphics
{
    public class Sprite3D : DrawablePrimitive3D
    {
        Vector3 position;
        Vector3 baseLeft;
        Vector3 baseUp;
        Vector3 up;
        Vector3 left;
        float scale;

        Color color;

        AnimatedTexture texture;

        VertexPositionColorTexture[] vertices;

        static public bool Initialized = false;
        static Effect effect = null;
        static VertexDeclaration declaration = null;
        static public void Initialize(XnaReference reference)
        {
            Initialized = true;
            effect = reference.Content.Load<Effect>("Effects/PositionColorTexture");
            declaration = new VertexDeclaration(reference.Device, VertexPositionColorTexture.VertexElements);
        }
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                vertices[0].Position = position + up + left;
                vertices[1].Position = position + up - left;
                vertices[2].Position = position - up - left;
                vertices[3].Position = position - up + left;
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
                vertices[0].Color = Color;
                vertices[1].Color = Color;
                vertices[2].Color = Color;
                vertices[3].Color = Color;
            }
        }
        public float Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                //Helper.Normalize(ref up);
                //Helper.Normalize(ref left);
                up = baseUp * scale;
                left = baseLeft * scale;
            }
        }
        Vector2 scale2;
        public Vector2 Scale2
        {
            get
            {
                return scale2;
            }
            set
            {
                scale2 = value;
                up = baseUp * scale2.Y;
                left = baseLeft * scale2.X;
            }
        }

        public Sprite3D(Vector3 position, Vector3 up, Vector3 left, Vector2 scale, Color color, AnimatedTexture texture)
        {
            this.vertices = new VertexPositionColorTexture[4];

            this.baseUp = up;
            this.baseLeft = left;

            Scale2 = scale;

            Position = position;
            Color = color;

            this.texture = texture;

            vertices[0].TextureCoordinate = new Vector2(0f, 1f);
            vertices[1].TextureCoordinate = new Vector2(1f, 1f);
            vertices[2].TextureCoordinate = new Vector2(1f, 0f);
            vertices[3].TextureCoordinate = new Vector2(0f, 0f);
        }

        public Sprite3D(Vector3 position, Vector3 up, Vector3 left, float scale, Color color, AnimatedTexture texture)
        {
            this.vertices = new VertexPositionColorTexture[4];

            this.baseUp = up;
            this.baseLeft = left;

            Scale = scale;
            Position = position;
            Color = color;
            this.texture = texture;

            vertices[0].TextureCoordinate = new Vector2(0f, 1f);
            vertices[1].TextureCoordinate = new Vector2(1f, 1f);
            vertices[2].TextureCoordinate = new Vector2(1f, 0f);
            vertices[3].TextureCoordinate = new Vector2(0f, 0f);
        }

        public void LeftUp(Vector3 left, Vector3 up)
        {
            this.baseLeft = left;
            this.baseUp = up;
            Scale = Scale;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, ChiseDrive.Cameras.Camera camera)
        {
            effect.CurrentTechnique = effect.Techniques[0];

            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(camera.View);
            effect.Parameters["Projection"].SetValue(camera.Projection);

            effect.Parameters["TextureMap"].SetValue(this.texture.Frame);

            device.VertexDeclaration = declaration;

            effect.Begin();

            for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
            {
                effect.CurrentTechnique.Passes[i].Begin();
                device.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleFan,
                    vertices, 0, 2);
                effect.CurrentTechnique.Passes[i].End();
            }

            effect.End();
        }
    }
}