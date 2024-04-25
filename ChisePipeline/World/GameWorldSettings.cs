using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChisePipeline.World
{
    public struct HeightmapLayer
    {
        public string Mask;
        public string Material;
    }

    public struct ScenePropSettings
    {
        public string Settings;
        public Vector3 Position;
        public Vector3 Rotation;
        public float Scale;
    }

    public struct HeightmapMeshSettings
    {
        public string Heightmap;
        public HeightmapLayer[] Layers;
        public Rectangle Size;
        public Vector2 TextureScale;
        public float HeightScale;
        public float HeightOffset;
    }

    public struct TextureSkyboxSettings
    {
        public string Texture;
        public Vector3 Position;
        public Vector3 LookAt;
        public Vector3 Up;
        public Vector2 Scale;
        public float Alpha;
    }

    public struct LightSourceSettings
    {
        public Color Color;
        public Vector3 Position;
    }

    public struct SunlightSettings
    {
        public Color Color;
        public Color Ambient;
        public Vector3 Position;
        public float Radius;
    }

    public class GameWorldSettings
    {
        public List<HeightmapMeshSettings> HeightmapMeshes = new List<HeightmapMeshSettings>();
        public List<ScenePropSettings> SceneProps = new List<ScenePropSettings>();
        public List<TextureSkyboxSettings> SkyboxTextures = new List<TextureSkyboxSettings>();
        public List<SunlightSettings> SunlightSource = new List<SunlightSettings>();
    }
}
