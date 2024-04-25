using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Cameras;

namespace ChiseDrive.Graphics
{
    public class SkyboxPane
    {
        static List<SkyboxPane> DrawList = new List<SkyboxPane>();
        static public void DrawAll(GraphicsDevice device, Camera camera, Visibility visibility)
        {
            if (DrawList.Count < 1) return;

            if (visibility == Visibility.Opaque) Effect.CurrentTechnique = Effect.Techniques["SkyBox"];
            else return;

            if (device == null) throw new NullReferenceException();
            if (camera == null) throw new NullReferenceException();
            
            Camera zoomcam = camera;// camera.CreateZoomedCamera(camera.Zoom * 0.8f); 

            SkyboxPane.Effect.Parameters["View"].SetValue(zoomcam.View);
            SkyboxPane.Effect.Parameters["Projection"].SetValue(zoomcam.Projection);

            device.RenderState.DepthBufferEnable = false;
            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            device.VertexDeclaration = SkyboxPane.VertexDeclaration;

            if (visibility == Visibility.Normal) device.RenderState.ReferenceAlpha = 254;        // Only render things with an alpha greater than this in the opaque pass

            foreach (SkyboxPane mesh in DrawList)
            {
                mesh.Draw(device, zoomcam);
            }

            if (visibility == Visibility.Normal) device.RenderState.ReferenceAlpha = 0;        // Only render things with an alpha greater than this in the opaque pass

            device.RenderState.DepthBufferEnable = true;
            device.RenderState.AlphaBlendEnable = false;
        }
        static public Effect Effect = null;
        static public VertexDeclaration VertexDeclaration = null;

        AnimatedTexture texture;
        private VertexPositionTexture[] vertices;
        Vector3 position;
        Vector3 baseLeft;
        Vector3 baseUp;
        Vector3 up;
        Vector3 left;
        float alpha;

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

        public Vector2 Scale
        {
            set
            {
                up = baseUp * value.Y;
                left = baseLeft * value.X;
            }
        }

        public SkyboxPane(AnimatedTexture texture, Vector3 position, Vector3 lookat, Vector3 up, Vector2 scale, float alpha)
        {
            this.vertices = new VertexPositionTexture[4];

            Matrix transform = Helper.RotateToFace(position, lookat, up);

            this.baseUp = transform.Up;
            this.baseLeft = transform.Left;

            Scale = scale;
            Position = position;

            this.texture = texture;

            this.alpha = alpha;
            this.texture = texture;
            this.texture.Loop();

            vertices[0].TextureCoordinate = new Vector2(0f, 0f);
            vertices[1].TextureCoordinate = new Vector2(1f, 0f);
            vertices[2].TextureCoordinate = new Vector2(1f, 1f);
            vertices[3].TextureCoordinate = new Vector2(0f, 1f);
        }

        public static float SkyboxMotionModifier = 0.03f;
        void Draw(GraphicsDevice device, Camera camera)
        {
            Matrix world = Matrix.CreateTranslation(Vector3.Zero);
            SkyboxPane.Effect.Parameters["World"].SetValue(world);

            device.RenderState.BlendFactor = new Color(255, 255, 255, (byte)(255.0f * alpha));
            SkyboxPane.Effect.Parameters["SkyTexture"].SetValue(texture.Frame);
            
            SkyboxPane.Effect.Begin();

            for (int i = 0; i < SkyboxPane.Effect.CurrentTechnique.Passes.Count; i++)
            {
                SkyboxPane.Effect.CurrentTechnique.Passes[i].Begin();
                device.DrawUserPrimitives(PrimitiveType.TriangleFan, vertices, 0, 2);
                SkyboxPane.Effect.CurrentTechnique.Passes[i].End();
            }

            SkyboxPane.Effect.End();
        }
    }
}