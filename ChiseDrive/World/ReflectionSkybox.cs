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
{/*
    public class ReflectionSkybox : IWorldComponent
    {
        SkyboxPane sky;

        public bool Visible 
        {
            get
            {
                return sky.Visible;
            }
            set
            {
                sky.Visible = value;
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

        public ReflectionSkybox(GraphicsDevice device, ContentManager content, Vector3 scale, String file)
        {
            AnimatedTexture texture = new AnimatedTexture(content, file + "sky");
            SkyboxPane.Effect = content.Load<Effect>("Effects/Skybox");
            SkyboxPane.VertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);
            sky = new SkyboxPane(texture,
                new Vector3((float)Math.PI, 0f, 0f),
                new Vector3(scale.X, scale.Y, scale.Z), 1f);
        }

        public void Dispose()
        {
            sky.Visible = false;
            sky = null;
        }

        public void Update(Time elapsed)
        {

        }

        public void Draw(GraphicsDevice device, Camera camera)
        {
            SkyboxPane.DrawAll(device, camera);
        }
    }*/
}