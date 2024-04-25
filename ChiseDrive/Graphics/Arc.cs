using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Cameras;

namespace ChiseDrive.Graphics
{
    public class Arc : DrawablePrimitive3D
    {
        static Effect effect;
        static VertexDeclaration declaration;
        static ChiseDriveGame Game;
        static public void Initialize(ChiseDriveGame game)
        {
            Game = game;
            effect = game.Content.Load<Effect>("Effects/FatLine");
            declaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionColor.VertexElements);
        }

        Color color;
        const int numvertexes = 2;
        const int maxsegments = 200;
        int segments = 0;
        VertexPositionColor[] vertex;
        float scale = 0.0f;

        public Arc(Color color)
        {
            this.color = color;
            vertex = new VertexPositionColor[maxsegments * numvertexes];
        }

        void FillSegment(Vector3 first, Vector3 second, int index)
        {
            Color drawcolor = this.color;

            if (index < 75)
            {
                drawcolor.A = (byte)(index * 2);
            }

            vertex[index + 0].Color = drawcolor;
            vertex[index + 1].Color = drawcolor;  
          
            Camera camera = Game.Camera;

            Vector3 forward = second - first;
            forward.Normalize();

            Vector3 tocamera = camera.Position - first;
            tocamera.Normalize();

            Vector3 width = Vector3.Cross(forward, tocamera);
            width *= 40f;

            vertex[index + 0].Position = first + width;
            vertex[index + 1].Position = second - width;
        }

        public void SetPoints(List<Vector3> points, float scale, float alpha)
        {
            this.scale = scale;

            for (segments = 0; segments < points.Count - 1 && segments < maxsegments; segments++)
            {
                FillSegment(points[segments], points[segments + 1], segments * numvertexes);
            }
        }

        public override void Draw(GraphicsDevice device, Camera camera)
        {
            if (segments <= 1) return;

            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(camera.View);
            effect.Parameters["Projection"].SetValue(camera.Projection);

            effect.Begin();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip,
                    vertex, 0, (segments * numvertexes) - 2);
                pass.End();
            }

            effect.End();
        }
    }
}