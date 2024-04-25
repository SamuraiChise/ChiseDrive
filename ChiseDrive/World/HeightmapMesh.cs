using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#if !Xbox
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endif
using ChiseDrive.Graphics;

namespace ChiseDrive.World
{
    public class HeightmapMesh : IWorldComponent, IDisposable
    {
        public bool Visible 
        {
            get
            {
                if (terrain == null) return false;
                else return terrain.Visible;
            }
            set
            {
                if (terrain != null) terrain.Visible = value;
            }
        }

        LitObject terrain = null;

        public HeightmapMesh() 
        {
        }

        public void Initialize(ChiseDriveGame game)
        {
            if (materialNames.Length > 0)
            {
                Effect effect = game.Content.Load<Effect>("Effects/HeightmapTerrain");
                VertexDeclaration declaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionNormalColorTextureMulti.VertexElements);
                CustomMultiTextureMesh mesh = new CustomMultiTextureMesh(game.GraphicsDevice, vertices, indices, declaration);
                terrain = new LitObject(game, mesh, Material.White, effect);
                terrain.Visibility = Visibility.Standard;

                for (int i = 0; i < materialNames.Length; i++)
                {
                    Material material = new Material(game.Content, materialNames[i]);
                    mesh.SetMaterial(material, i);
                }
            }
        }

        public void Dispose()
        {
            Visible = false;

            if (terrain != null)
            {
                terrain.Dispose();
            }
            terrain = null;
        }

        public struct HeightmapSettings
        {
            public int DataWidth;
            public int DataHeight;
            public float OffsetX;
            public float OffsetY;
            public float OffsetZ; 
            public float ScaleZ;            
            public float Width;//Width as float
            public float Length;//Length as float
            public float SpanX;//UnitsPerIndex
            public float SpanY;//UnitsPerIndex
            public float TextureX;
            public float TextureY;
        }

        #region Read/Writeable
        HeightmapSettings settings;
        VertexPositionNormalColorTextureMulti[] vertices;
        int[] indices;
        string[] materialNames;
        #endregion

        public void Build(VertexPositionNormalColorTextureMulti[] vertices, int[] indices, string[] materialNames, HeightmapSettings settings)
        {
            this.vertices = vertices;
            this.indices = indices;
            this.materialNames = materialNames;
            this.settings = settings;
        }

        public Vector3 CorrectForBounds(Vector3 initial)
        {
            if (initial.X < settings.OffsetX) initial.X = settings.OffsetX;
            if (initial.X >= settings.OffsetX + settings.Width) initial.X = settings.OffsetX + settings.Width - 1f;
            if (initial.Y < settings.OffsetY) initial.Y = settings.OffsetY;
            if (initial.Y >= settings.OffsetY + settings.Length) initial.Y = settings.OffsetY + settings.Length - 1f;

            return initial;
        }

        public Vector3 CorrectForHeight(Vector3 initial)
        {
            // Bounds check to see if we have height control over the point
            if (initial.X < settings.OffsetX
                || initial.Y < settings.OffsetY
                || initial.X >= settings.OffsetX + settings.Width
                || initial.Y >= settings.OffsetY + settings.Length)
                return initial;

            int x1 = (int)((initial.X - settings.OffsetX) / settings.SpanX);
            int y1 = (int)((initial.Y - settings.OffsetY) / settings.SpanY);
            int x2 = x1 + 1;
            int y2 = y1 + 1;

            if (x1 < 0) x1 = 0;
            if (y1 < 0) y1 = 0;
            if (x1 >= settings.DataWidth) x1 = settings.DataWidth - 1;
            if (y1 >= settings.DataHeight) y1 = settings.DataHeight - 1;

            if (x2 < 0) x2 = 0;
            if (y2 < 0) y2 = 0;
            if (x2 >= settings.DataWidth) x2 = settings.DataWidth - 1;
            if (y2 >= settings.DataHeight) y2 = settings.DataHeight - 1;

            if (x1 == x2) x1--;
            if (y1 == y2) y1--;

            Vector2 testpoint = new Vector2((initial.X - settings.OffsetX) / settings.SpanX, 
                (initial.Y - settings.OffsetY) / settings.SpanY);

            Vector2 corner1 = new Vector2((float)x1, (float)y1);
            Vector2 corner2 = new Vector2((float)x2, (float)y2);

            float xPercent = (testpoint.X - corner1.X) / (corner2.X - corner1.X);
            float yPercent = (testpoint.Y - corner1.Y) / (corner2.Y - corner1.Y);

            float weight11 = (1f - xPercent) * (1f - yPercent);
            float weight12 = (1f - xPercent) * (yPercent);
            float weight21 = (xPercent) * (1f - yPercent);
            float weight22 = (xPercent) * (yPercent);

            float z11 = vertices[x1 + (y1 * settings.DataWidth)].Position.Z * weight11;
            float z12 = vertices[x1 + (y2 * settings.DataWidth)].Position.Z * weight12;
            float z21 = vertices[x2 + (y1 * settings.DataWidth)].Position.Z * weight21;
            float z22 = vertices[x2 + (y2 * settings.DataWidth)].Position.Z * weight22;

            Vector3 retvalue = initial;
            retvalue.Z = z11 + z12 + z21 + z22;

            return retvalue;
        }

        public void SizeBounds(ref Rectangle rectangle)
        {
            if (rectangle.X > settings.OffsetX) rectangle.X = (int)settings.OffsetX;
            if (rectangle.Y > settings.OffsetY) rectangle.Y = (int)settings.OffsetY;

            int dx = (int)(settings.OffsetX - rectangle.X);
            int dy = (int)(settings.OffsetY - rectangle.Y);

            int width = dx + (int)settings.Width;
            int height = dy + (int)settings.Length;

            if (rectangle.Width < width) rectangle.Width = width;
            if (rectangle.Height < height) rectangle.Height = height;
        }

        public void Update(Time elapsed)
        {
            // TODO: Check for on-screen visiblity and add/remove from the visible
            // list accordingly!
        }

#if !Xbox
        public void Write(ContentWriter output)
        {
            output.WriteObject<VertexPositionNormalColorTextureMulti[]>(vertices);
            output.WriteObject<int[]>(indices);
            output.WriteObject<string[]>(materialNames);
            output.Write(settings.DataWidth);
            output.Write(settings.DataHeight);
            output.Write(settings.TextureX);
            output.Write(settings.TextureY);
            output.Write(settings.OffsetX);
            output.Write(settings.OffsetY);
            output.Write(settings.OffsetZ);
            output.Write(settings.ScaleZ);
            output.Write(settings.Width);
            output.Write(settings.Length);
            output.Write(settings.SpanX);
            output.Write(settings.SpanY);
        }
#endif
        public void Read(ContentReader input)
        {
            vertices = input.ReadObject<VertexPositionNormalColorTextureMulti[]>();
            indices = input.ReadObject<int[]>();
            materialNames = input.ReadObject<string[]>();
            settings.DataWidth = input.ReadInt32();
            settings.DataHeight = input.ReadInt32();
            settings.TextureX = input.ReadSingle();
            settings.TextureY = input.ReadSingle();
            settings.OffsetX = input.ReadSingle();
            settings.OffsetY = input.ReadSingle();
            settings.OffsetZ = input.ReadSingle();
            settings.ScaleZ = input.ReadSingle();
            settings.Width = input.ReadSingle();
            settings.Length = input.ReadSingle();
            settings.SpanX = input.ReadSingle();
            settings.SpanY = input.ReadSingle();
        }
    }
}
