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
    public class SpaceSkybox : IWorldComponent
    {
        SkyboxPane[] space;

        public struct Data
        {
            public string file;
            public Vector3 yawpitchroll;
            public float scale;
            public float alpha;
        }
        static public Data Create(string file, Vector3 yawpitchroll, float scale, float alpha)
        {
            Data data = new Data();
            data.file = file;
            data.yawpitchroll = yawpitchroll;
            data.scale = scale;
            data.alpha = alpha;
            return data;
        }

        public PointLight Light { get { return null; } }

        public bool Visible
        {
            get
            {
                return space[0].Visible;
            }
            set
            {
                foreach (SkyboxPane pane in space)
                {
                    pane.Visible = true;
                }
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

        public SpaceSkybox(GraphicsDevice device, ContentManager content, Data[] data)
        {
            space = new SkyboxPane[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                space[i] = new SkyboxPane(new AnimatedTexture(content, data[i].file), data[i].yawpitchroll, data[i].scale, data[i].alpha);
            }

            SkyboxPane.Effect = content.Load<Effect>("Effects/Skybox");
            SkyboxPane.VertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);
        }

        public void Dispose()
        {
            for (int i = 0; i < space.Length; i++)
            {
                space[i].Visible = false;
                space[i] = null;
            }
            space = null;
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