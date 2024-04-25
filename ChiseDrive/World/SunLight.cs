using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChiseDrive.Graphics;
using ChiseDrive.Cameras;
using Microsoft.Xna.Framework.Content;
#if !Xbox
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChiseDrive.World
{
    public class SunLight : IWorldComponent
    {
        PointLight lightsource;
        Color ambient;

        public bool Visible 
        {
            get
            {
                return lightsource.Visible;
            }
            set
            {
                lightsource.Visible = value;
            }
        }
        public PointLight Light
        {
            get
            {
                return lightsource;
            }
        }

        public SunLight(PointLight sunlight)
        {
            Color ambient = sunlight.Color;
            ambient.R /= 4;
            ambient.G /= 4;
            ambient.B /= 4;
            Vector3 p = new Vector3(sunlight.Position.X, sunlight.Position.Y, sunlight.Position.Z);
            Initialize(sunlight.Color, ambient, p, float.PositiveInfinity);
        }

        public void Initialize(ChiseDriveGame game)
        {
            LitObject.AmbientLight = ambient;
        }

        public readonly static Color SunlightColor = new Color(240, 240, 188);
        public readonly static Color MoonlightColor = new Color(100, 100, 220);

        public SunLight()
        {
        }

        public SunLight(Color color, Color ambient, Vector3 position, float radius)
        {
            Initialize(color, ambient, position, radius);
        }

        public SunLight(Color color)
        {
            Color ambient = color;
            ambient.R /= 4;
            ambient.G /= 4;
            ambient.B /= 4;
            Initialize(color, ambient, new Vector3(2000f, -60000f, -60000f), float.PositiveInfinity);
        }

        void Initialize(Color color, Color ambient, Vector3 position, float radius)
        {
            PointLight sunlight = new PointLight(new Vector4(position, 16000f), color);
            sunlight.Range = radius;
            sunlight.Falloff = 2f;

            this.lightsource = sunlight;
            this.lightsource.Scope = LightScope.Global;

            this.ambient = ambient;
        }

        public void Dispose()
        {
            Visible = false;
            this.lightsource.Dispose();
            this.lightsource = null;
        }

        public void Update(Time elapsed) { }
        public Vector3 CorrectForHeight(Vector3 initial) { return initial; }
        public Vector3 CorrectForBounds(Vector3 initial) { return GameWorld.InvalidPosition; }
        public void SizeBounds(ref Rectangle rectangle) { }
        #if !Xbox
        public void Write(ContentWriter output)
        {
            output.Write(ambient);
            output.Write(lightsource.Position);
            output.Write(lightsource.Color);
            output.Write(lightsource.Range);
        }
#endif
        public void Read(ContentReader input)
        {
            ambient = input.ReadColor();

            Vector4 pos = input.ReadVector4();
            Color col = input.ReadColor();
            float radius = input.ReadSingle();
            lightsource = new PointLight(pos, col);
            lightsource.Range = radius;
            lightsource.Falloff = 2f;
            lightsource.Scope = LightScope.Global;
        }
    }
}