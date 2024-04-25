using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using ChiseDrive.World;
using ChiseDrive.Graphics;
using ChiseDrive.Units;

// TODO: replace these with the processor input and output types.
using TInput = ChisePipeline.World.GameWorldSettings;
using TOutput = ChiseDrive.World.GameWorld;

namespace ChisePipeline.World
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "GameWorldProcessor")]
    public class GameWorldProcessor : ContentProcessor<TInput, TOutput>
    {
        public override TOutput Process(TInput input, ContentProcessorContext context)
        {
            ChiseDrive.World.GameWorld World = new GameWorld();

            foreach (SunlightSettings lss in input.SunlightSource)
            {
                SunLight light = new SunLight(lss.Color, lss.Ambient, lss.Position, lss.Radius);
                World.Components.Add(light);
            }

            foreach (TextureSkyboxSettings tss in input.SkyboxTextures)
            {
                TextureSkybox skybox = new TextureSkybox(
                    tss.Texture, tss.Position, tss.LookAt, tss.Up, tss.Scale, tss.Alpha);
                World.Components.Add(skybox);
            }

            foreach (ScenePropSettings prop in input.SceneProps)
            {
                BodySettings settings = context.BuildAndLoadAsset<BodySettings, BodySettings>(
                    new ExternalReference<BodySettings>(prop.Settings), null);
                settings.Scale = prop.Scale;
                SceneObject sceneobject = new SceneObject(settings, prop.Position, prop.Rotation);

                World.Components.Add(sceneobject);
            }

            HeightmapMesh[] heightmapMeshes = new HeightmapMesh[input.HeightmapMeshes.Count];
            foreach (HeightmapMeshSettings hms in input.HeightmapMeshes)
            {
                HeightmapMesh terrain = new HeightmapMesh();

                Texture2DContent heightmap = context.BuildAndLoadAsset<Texture2DContent, Texture2DContent>(
                    new ExternalReference<Texture2DContent>(hms.Heightmap), null);

                Texture2DContent mask0 = hms.Layers.Length < 1 ? null : 
                    context.BuildAndLoadAsset<Texture2DContent, Texture2DContent>(
                    new ExternalReference<Texture2DContent>(hms.Layers[0].Mask), null);

                Texture2DContent mask1 = hms.Layers.Length < 2 ? null : 
                    context.BuildAndLoadAsset<Texture2DContent, Texture2DContent>(
                    new ExternalReference<Texture2DContent>(hms.Layers[1].Mask), null);

                Texture2DContent mask2 = hms.Layers.Length < 3 ? null : 
                    context.BuildAndLoadAsset<Texture2DContent, Texture2DContent>(
                    new ExternalReference<Texture2DContent>(hms.Layers[2].Mask), null);

                Texture2DContent mask3 = hms.Layers.Length < 4 ? null : 
                    context.BuildAndLoadAsset<Texture2DContent, Texture2DContent>(
                    new ExternalReference<Texture2DContent>(hms.Layers[3].Mask), null);

                //Texture2DContent[] masks = new Texture2DContent[hms.Layers.Length];
                string[] materials = new string[hms.Layers.Length];

                for (int i = 0; i < hms.Layers.Length; i++)
                {
                    //masks[i] = context.BuildAndLoadAsset<Texture2DContent, Texture2DContent>(
                    //    new ExternalReference<Texture2DContent>(hms.Layers[i].Mask), null);

                    materials[i] = hms.Layers[i].Material;
                }
                
                HeightmapMesh.HeightmapSettings settings =
                    BuildSettingsFromMap(heightmap, hms);

                VertexPositionNormalColorTextureMulti[] vertices = 
                    BuildVerticesFromMask(heightmap, mask0, mask1, mask2, mask3, settings);

                int[] indices = BuildIndices(settings);

                CalculateNormals(vertices, indices);
                terrain.Build(vertices, indices, materials, settings);

                World.Components.Add(terrain);
            }


            return World;
        }

        HeightmapMesh.HeightmapSettings BuildSettingsFromMap(Texture2DContent heightmap, HeightmapMeshSettings hms)
        {
            HeightmapMesh.HeightmapSettings settings;

            settings.DataWidth = heightmap.Mipmaps[0].Width;
            settings.DataHeight = heightmap.Mipmaps[0].Height;

            settings.OffsetX = (float)hms.Size.X;
            settings.OffsetY = (float)hms.Size.Y;
            settings.Width = (float)hms.Size.Width;
            settings.Length = (float)hms.Size.Height;

            settings.OffsetZ = (float)hms.HeightOffset;
            settings.ScaleZ = (float)hms.HeightScale;
            settings.TextureX = hms.TextureScale.X;
            settings.TextureY = hms.TextureScale.Y;
            settings.SpanX = settings.Width / (float)settings.DataWidth;
            settings.SpanY = settings.Length / (float)settings.DataHeight;

            return settings;
        }

        VertexPositionNormalColorTextureMulti[] BuildVerticesFromMask(
            Texture2DContent heightmap, 
            Texture2DContent mask0,
            Texture2DContent mask1,
            Texture2DContent mask2,
            Texture2DContent mask3,
            HeightmapMesh.HeightmapSettings settings)
        {
            int width = settings.DataWidth;
            int length = settings.DataHeight;

            VertexPositionNormalColorTextureMulti[] vertices = 
                new VertexPositionNormalColorTextureMulti[width * length];

            heightmap.ConvertBitmapType(typeof(PixelBitmapContent<float>));
            PixelBitmapContent<float> heightdata;
            heightdata = (PixelBitmapContent<float>)heightmap.Mipmaps[0];

            PixelBitmapContent<float> maskdata0 = null;
            PixelBitmapContent<float> maskdata1 = null;
            PixelBitmapContent<float> maskdata2 = null;
            PixelBitmapContent<float> maskdata3 = null;

            if (mask0 != null)
            {
                mask0.ConvertBitmapType(typeof(PixelBitmapContent<float>));
                maskdata0 = (PixelBitmapContent<float>)mask0.Mipmaps[0];
            }

            if (mask1 != null)
            {
                mask1.ConvertBitmapType(typeof(PixelBitmapContent<float>));
                maskdata1 = (PixelBitmapContent<float>)mask1.Mipmaps[0];
            }

            if (mask2 != null)
            {
                mask2.ConvertBitmapType(typeof(PixelBitmapContent<float>));
                maskdata2 = (PixelBitmapContent<float>)mask2.Mipmaps[0];
            }

            if (mask3 != null)
            {
                mask3.ConvertBitmapType(typeof(PixelBitmapContent<float>));
                maskdata3 = (PixelBitmapContent<float>)mask3.Mipmaps[0];
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    int key = x + (y * width);

                    vertices[key].Position = new Vector3(
                        settings.OffsetX + x * settings.SpanX,
                        settings.OffsetY + y * settings.SpanY,
                        heightdata.GetPixel(x, y) * settings.ScaleZ + settings.OffsetZ);

                    vertices[key].Color = Color.White.ToVector4();

                    vertices[key].TextureCoordinate.X = (float)x * settings.TextureX / (float)width;
                    vertices[key].TextureCoordinate.Y = (float)(length - y - 1) * settings.TextureY / (float)length;

                    vertices[key].TexWeights.X = maskdata0 == null ? 0f : maskdata0.GetPixel(x, y);
                    vertices[key].TexWeights.Y = maskdata1 == null ? 0f : maskdata1.GetPixel(x, y);
                    vertices[key].TexWeights.Z = maskdata2 == null ? 0f : maskdata2.GetPixel(x, y);
                    vertices[key].TexWeights.W = maskdata3 == null ? 0f : maskdata3.GetPixel(x, y);

                    vertices[key].TexWeights.Normalize();
                }
            }

            return vertices;
        }


        int[] BuildIndices(HeightmapMesh.HeightmapSettings settings)
        {
            int[] indices = new int[(settings.DataWidth - 1) * (settings.DataHeight - 1) * 6];
            int counter = 0;

            for (int y = 0; y < settings.DataHeight - 1; y++)
            {
                for (int x = 0; x < settings.DataWidth - 1; x++)
                {
                    int lowerLeft = x + y * settings.DataWidth;
                    int lowerRight = (x + 1) + y * settings.DataWidth;
                    int upperLeft = x + (y + 1) * settings.DataWidth;
                    int upperRight = (x + 1) + (y + 1) * settings.DataWidth;

                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;
                    indices[counter++] = upperLeft;

                    indices[counter++] = lowerRight;
                    indices[counter++] = upperLeft;
                    indices[counter++] = upperRight;

                    //indices[counter++] = upperLeft;
                    //indices[counter++] = lowerRight;
                    //indices[counter++] = lowerLeft;

                    //indices[counter++] = upperLeft;
                    //indices[counter++] = upperRight;
                    //indices[counter++] = lowerRight;
                }
            }

            return indices;
        }

        void CalculateNormals(VertexPositionNormalColorTextureMulti[] vertices, int[] indices)
        {
            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                vertices[index1].Normal = new Vector3(0, 0, 0);
                vertices[index2].Normal = new Vector3(0, 0, 0);
                vertices[index3].Normal = new Vector3(0, 0, 0);

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;

                vertices[index1].Normal.Normalize();
                vertices[index2].Normal.Normalize();
                vertices[index3].Normal.Normalize();
            }
        }
    }
}