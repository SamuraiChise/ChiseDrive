using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Cameras;
using ChiseDrive.World;

namespace ChiseDrive.Graphics
{
    class WaterSurface : LitObject
    {
        CustomMesh mesh;

        public Texture2D ReflectionMap;
        public Texture2D RefractionMap;
        public Texture2D WaterBumpMap;
        public Matrix ReflectionView;
        public Wind Wind;

        public WaterSurface(ChiseDriveGame game, CustomMesh mesh, Effect effect)
            : base(game, effect)
        {
            this.mesh = mesh;
            this.mesh.Effect = Effect;
            this.Wind = new Wind(new Vector4(1f, 1f, 0f, 2f));
        }

        public override void Draw(GraphicsDevice device, Camera camera, Visibility pass)
        {
            device.RenderState.DepthBufferWriteEnable = true;
            device.RenderState.DepthBufferEnable = true;
            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.BlendFunction = BlendFunction.Add;

            Effect.CurrentTechnique = Effect.Techniques["Water"];
            Effect.Parameters["xView"].SetValue(camera.View);
            Effect.Parameters["xReflectionView"].SetValue(ReflectionView);
            Effect.Parameters["xProjection"].SetValue(camera.Projection);
            Effect.Parameters["xReflectionMap"].SetValue(ReflectionMap);
            Effect.Parameters["xRefractionMap"].SetValue(RefractionMap);
            Effect.Parameters["xWaterBumpMap"].SetValue(WaterBumpMap);
            Effect.Parameters["xWaveLength"].SetValue(0.1f * Wind.Intensity);
            Effect.Parameters["xWaveHeight"].SetValue(0.3f * Wind.Intensity);
            Effect.Parameters["xCamPos"].SetValue(camera.Position);
            Effect.Parameters["xWindForce"].SetValue(Wind.Percent);
            Effect.Parameters["xWindDirection"].SetValue(Wind.Direction);

            Effect.Begin();
            foreach (EffectPass effectpass in Effect.CurrentTechnique.Passes)
            {
                effectpass.Begin();

                mesh.Draw(device, camera);

                effectpass.End();
            }
            Effect.End();
        }

        /*
        public WaterSurface(GraphicsDevice device, VertexPositionTexture[] vertices, Effect effect, VertexDeclaration declaration)
        {
            this.effect = effect;
            this.declaration = declaration;
            this.Wind = new Wind(Vector4.Zero);

            vertexBuffer = new VertexBuffer(device, vertices.Length * VertexPositionTexture.SizeInBytes, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
        }

        public void Draw(GraphicsDevice device, Camera camera)
        {
            effect.CurrentTechnique = effect.Techniques["Water"];
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(camera.View);
            effect.Parameters["xReflectionView"].SetValue(ReflectionView);
            effect.Parameters["xProjection"].SetValue(camera.Projection);
            effect.Parameters["xReflectionMap"].SetValue(ReflectionMap);
            effect.Parameters["xRefractionMap"].SetValue(RefractionMap);
            effect.Parameters["xWaterBumpMap"].SetValue(WaterBumpMap);
            effect.Parameters["xWaveLength"].SetValue(0.1f * Wind.Intensity);
            effect.Parameters["xWaveHeight"].SetValue(0.3f * Wind.Intensity);
            effect.Parameters["xCamPos"].SetValue(camera.Position);
            effect.Parameters["xWindForce"].SetValue(Wind.Percent);
            effect.Parameters["xWindDirection"].SetValue(Wind.Direction);

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionTexture.SizeInBytes);
                device.VertexDeclaration = declaration;
                int noVertices = vertexBuffer.SizeInBytes / VertexPositionTexture.SizeInBytes;
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, noVertices / 3);

                pass.End();
            }
            effect.End();
        }*/
    }
}