using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ChiseDrive.Graphics;
using ChiseDrive.Cameras;

namespace ChiseDrive.World
{

    /*
    public class HeightmapTerrain : IWorldComponent, ILitObject
    {
        int width;
        int length;
        float[,] heightData;
        Vector3 scale;
        Vector2 texscale;

        LitObject terrain;

        GraphicsDevice device;

        public Rectangle Dimensions
        {
            get
            {
                Rectangle retvalue = new Rectangle();
                retvalue.X = (int)(0f - ((float)width / 2f) * scale.X);
                retvalue.Y = (int)(0f - ((float)width / 2f) * scale.Y);
                retvalue.Width = (int)((float)width * scale.X);
                retvalue.Height = (int)((float)length * scale.Y);
                return retvalue;
            }
        }
        public bool Visible
        {
            get
            {
                return terrain.Visible;
            }
            set
            {
                terrain.Visible = value;
            }
        }
        public IWeather Weather
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public PointLight Light { get { return null; } }

        public HeightmapTerrain(ChiseDriveGame game, Vector3 scale, Vector2 texscale, String folder)
        {
            this.device = game.GraphicsDevice;
            Effect effect = game.Content.Load<Effect>("Effects/HeightmapTerrain");

            Texture2D heightmap = game.Content.Load<Texture2D>(folder + "heightmap");

            this.scale.X = scale.X;
            this.scale.Y = scale.Y;
            this.scale.Z = scale.Z;
            this.texscale = texscale;

            this.width = heightmap.Width;
            this.length = heightmap.Height;

            heightData = GetHeightFromMap(heightmap, scale.Z);

            Texture2D mask0 = game.Content.Load<Texture2D>(folder + "texturemask0");
            Texture2D mask1 = game.Content.Load<Texture2D>(folder + "texturemask1");
            Texture2D mask2 = game.Content.Load<Texture2D>(folder + "texturemask2");
            Texture2D mask3 = game.Content.Load<Texture2D>(folder + "texturemask3");

            VertexDeclaration declaration = new VertexDeclaration(device, VertexPositionNormalColorTextureMulti.VertexElements);

            VertexPositionNormalColorTextureMulti[] terrainVertices =
                BuildVerticesFromMask(heightmap, mask0, mask1, mask2, mask3);
            int[] terrainIndices = BuildTerrainIndices();

            CustomMultiTextureMesh mesh = new CustomMultiTextureMesh(game.GraphicsDevice, terrainVertices, terrainIndices, declaration);
            
            string[] n = { "d", "s", "n" };

            Material material = new Material(game.Content, "test");
            terrain = new LitObject(game, mesh, material, effect);
            terrain.Visibility = Visibility.Standard;
            terrain.Visible = true;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    string name = i + n[j];
                    mesh.SetTexture(game.Content.Load<Texture2D>(folder + "texture" + i + n[j]), i, j, name);
                }
            }

            Texture2D occupancy = game.Content.Load<Texture2D>(folder + "occupancy");
        }

        public void Dispose()
        {
            terrain.Visible = false;
            terrain.Dispose();

            device = null;
            heightData = null;
        }

        public void AddLight(PointLight light)
        {
            terrain.AddLight(light);
        }
        public void RemoveLight(PointLight light)
        {
            terrain.RemoveLight(light);
        }

        float[,] GetHeightFromMap(Texture2D heightMap, float scale)
        {
            Color[] rawcolor = new Color[width * length];
            heightMap.GetData(rawcolor);

            float[,] heightarray = new float[width, length];

            for (int w = 0; w < width; w++)
            {
                for (int l = 0; l < length; l++)
                {
                    heightarray[w, l] = (float)rawcolor[w + l * length].R;
                }
            }

            return heightarray;
        }

        /// <summary>
        /// Builds the vertice alpha values from the mask.
        /// </summary>
        /// <param name="maskmap"></param>
        /// <returns></returns>
        VertexPositionNormalColorTextureMulti[] BuildVerticesFromMask(Texture2D maskmap, Texture2D mask0, Texture2D mask1, Texture2D mask2, Texture2D mask3)
        {
            VertexPositionNormalColorTextureMulti[] vertices = new VertexPositionNormalColorTextureMulti[width * length];

            Color[] heightColor = new Color[width * length];
            Color[] maskColor0 = new Color[width * length];
            Color[] maskColor1 = new Color[width * length];
            Color[] maskColor2 = new Color[width * length];
            Color[] maskColor3 = new Color[width * length];

            maskmap.GetData(heightColor);
            mask0.GetData(maskColor0);
            mask1.GetData(maskColor1);
            mask2.GetData(maskColor2);
            mask3.GetData(maskColor3);

            float wc = width / 2f;
            float lc = length / 2f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    int key = x + (y * width);

                    vertices[key].Position = new Vector3(
                        (x - wc) * scale.X,
                        (y - lc) * scale.Y,
                        heightData[x, y] * scale.Z);

                    vertices[key].TextureCoordinate.X = x * texscale.X;
                    vertices[key].TextureCoordinate.Y = y * texscale.Y;

                    Vector4 weights = new Vector4();
                    weights.X = (float)maskColor0[key].A;
                    weights.Y = (float)maskColor1[key].A;
                    weights.Z = (float)maskColor2[key].A;
                    weights.W = (float)maskColor3[key].A;

                    vertices[key].Color = Color.White.ToVector4();

                    float total = weights.W + weights.X + weights.Y + weights.Z;
                    weights /= total;

                    vertices[key].TexWeights.W = weights.W;
                    vertices[key].TexWeights.X = weights.X;
                    vertices[key].TexWeights.Y = weights.Y;
                    vertices[key].TexWeights.Z = weights.Z;
                }
            }

            return vertices;
        }

        public Vector3 CorrectForHeight(Vector3 initial)
        {
            Vector3 testvalue = initial;

            testvalue /= scale;
            float wc = width / 2f;
            float lc = length / 2f;

            int x1 = (int)(testvalue.X + wc);
            int y1 = (int)(testvalue.Y + lc);
            int x2 = (int)(testvalue.X + wc + 1f);
            int y2 = (int)(testvalue.Y + lc + 1f);

            if (x1 == x2) x1--;
            if (y1 == y2) y1--;

            if (x1 < 0) x1 = 0;
            if (y1 < 0) y1 = 0;
            if (x1 >= width) x1 = width-1;
            if (y1 >= length) y1 = length-1;

            if (x2 < 0) x2 = 0;
            if (y2 < 0) y2 = 0;
            if (x2 >= width) x2 = width - 1;
            if (y2 >= length) y2 = length - 1;

            Vector2 testpoint = new Vector2(testvalue.X + wc, testvalue.Y + lc);

            Vector2 corner1 = new Vector2((float)x1, (float)y1);
            Vector2 corner2 = new Vector2((float)x2, (float)y2);

            float xPercent = (testpoint.X - corner1.X) / (corner2.X - corner1.X);
            float yPercent = (testpoint.Y - corner1.Y) / (corner2.Y - corner1.Y);

            float weight11 = (1f - xPercent) * (1f - yPercent);
            float weight12 = (1f - xPercent) * (yPercent);
            float weight21 = (xPercent) * (1f - yPercent);
            float weight22 = (xPercent) * (yPercent);

            float z11 = heightData[x1, y1] * scale.Z * weight11;
            float z12 = heightData[x1, y2] * scale.Z * weight12;
            float z21 = heightData[x2, y1] * scale.Z * weight21;
            float z22 = heightData[x2, y2] * scale.Z * weight22;

            
            Vector3 retvalue = initial;
            retvalue.Z = z11 + z12 + z21 + z22;

            return retvalue;
        }

        int[] BuildTerrainIndices()
        {
            int[] indices = new int[(width - 1) * (length - 1) * 6];
            int counter = 0;

            for (int y = 0; y < length - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int lowerLeft = x + y * width;
                    int lowerRight = (x + 1) + y * width;
                    int topLeft = x + (y + 1) * width;
                    int topRight = (x + 1) + (y + 1) * width;

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }

            return indices;
        }

        public void Update(Time elapsed)
        {
        }
    }*/
}