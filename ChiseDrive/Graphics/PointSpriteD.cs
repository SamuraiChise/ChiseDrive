using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ChiseDrive.Cameras;

namespace ChiseDrive.Graphics
{
    public class PointSpriteD
    {
        static List<PointSpriteD> DrawList = new List<PointSpriteD>();
        static public void DrawAll(GraphicsDevice device, Camera camera)
        {
            if (device == null) throw new NullReferenceException();
            if (camera == null) throw new NullReferenceException();

            device.RenderState.DepthBufferEnable = true;
            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

            foreach (PointSpriteD sprite in DrawList)
            {
                sprite.Draw(device, camera);
            }
        }

        public bool Visible
        {
            get
            {
                return DrawList.Contains(this);
            }
            set
            {
                if (value && !Visible) DrawList.Add(this);
                if (!value && Visible) DrawList.Remove(this);
            }
        }

        static private Effect PointSpriteEffect = null;
        struct VertexPositionColorScale
        {
            public Vector3 position;
            public Color color;
            public float scale;

            public VertexPositionColorScale(Vector3 position, Color color, float scale)
            {
                this.position = position;
                this.color = color;
                this.scale = scale;
            }

            public static VertexElement[] VertexElements =
                {
                    new VertexElement(0, 0, 
                    VertexElementFormat.Vector3, 
                    VertexElementMethod.Default,
                    VertexElementUsage.Position,
                    0),
                    new VertexElement(0, sizeof(float)*3,
                    VertexElementFormat.Color,
                    VertexElementMethod.Default,
                    VertexElementUsage.Color,
                    0),
                    new VertexElement(0, sizeof(float)*4,
                    VertexElementFormat.Single,
                    VertexElementMethod.Default,
                    VertexElementUsage.PointSize,
                    0)
                };
            public static int SizeInBytes
            {
                get
                {
                    return sizeof(float) * 5;
                }
            }
        }

        AnimatedTexture texture = null;
        int totalvertex = 1;
        int vertexcount = 0;
        VertexPositionColorScale[] vertex;
        static VertexDeclaration declaration = null;
        float scale = 0.0f;

        public AnimatedTexture Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
            }
        }

        public PointSpriteD(AnimatedTexture texture, XnaReference reference)
        {
            if (PointSpriteEffect == null) PointSpriteEffect = reference.Content.Load<Effect>("Effects/ScaledPointSprite");
            if (declaration == null) declaration = new VertexDeclaration(reference.Device, VertexPositionColorScale.VertexElements);
            vertex = new VertexPositionColorScale[totalvertex];
            this.texture = texture;
            vertex[0].color = Color.White;
        }

        public void Position(Vector3 position, float scale, float alpha)
        {
            this.scale = scale;
            if (vertexcount >= totalvertex) vertexcount = totalvertex - 1;
            vertex[vertexcount].position = position;
            vertex[vertexcount].scale = this.scale;
            vertexcount++;
        }

        /// <summary>
        /// Draws the animation on a 3D point sprite inside the render world.
        /// </summary>
        /// <param name="position">3D position.</param>
        /// <param name="scale">Size of the sprite.</param>
        /// <param name="alpha">Destination transparency blend.</param>
        public void Draw(GraphicsDevice device, Camera camera)
        {
            device.RenderState.PointSpriteEnable = true;

            device.RenderState.AlphaBlendEnable = true;

            device.RenderState.DepthBufferEnable = true;
            device.RenderState.DepthBufferWriteEnable = false;

            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.BlendFunction = BlendFunction.Add;
            // Xbox 360 is picky about the Blend modes supported, Blend.One tends to be it's favorite
            device.RenderState.DestinationBlend = Blend.One;

            device.VertexDeclaration = declaration;

            PointSpriteEffect.Parameters["World"].SetValue(Matrix.Identity);
            PointSpriteEffect.Parameters["View"].SetValue(camera.View);
            PointSpriteEffect.Parameters["Projection"].SetValue(camera.Projection);

            if (vertexcount == 0) return;

            PointSpriteEffect.Parameters["SpriteTexture"].SetValue(texture.Frame);

            PointSpriteEffect.Begin();

            foreach (EffectPass pass in PointSpriteEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawUserPrimitives<VertexPositionColorScale>(PrimitiveType.PointList,
                    vertex, 0, vertexcount);
                pass.End();
            }

            // Reset the vertex count because they have to be re-queued to draw next frame
            vertexcount = 0;

            PointSpriteEffect.End();
            //SolInvasion.DebugText("Point Sprite (" + SpriteArray[0].Position.X + ", " + SpriteArray[0].Position.Y + ", " + SpriteArray[0].Position.Z + ")");

            device.RenderState.PointSpriteEnable = false;
            device.RenderState.AlphaBlendEnable = false;

            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.One;

            device.RenderState.DepthBufferWriteEnable = true;
        }
    }
}
