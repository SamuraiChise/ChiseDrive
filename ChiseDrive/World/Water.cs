using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ChiseDrive.Cameras;
using ChiseDrive.Graphics;

namespace ChiseDrive.World
{/*
    public class Water : IWorldComponent
    {
        public float Height = 1f;
        RenderTarget2D refractionRenderTarget;
        RenderTarget2D reflectionRenderTarget;

        public PointLight Light { get { return null; } }
        WaterSurface mesh;

        public bool Visible
        {
            get
            {
                return mesh.Visible;
            }
            set
            {
                mesh.Visible = value;
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
                if (value is Weather) mesh.Wind = (value as Weather).Wind;
            }
        }

        enum DrawPlane
        {
            Above,
            Below,
        };

        public Water(ChiseDriveGame game, Vector3 scale, String folder)
        {
            PresentationParameters pp = game.GraphicsDevice.PresentationParameters;
            refractionRenderTarget = new RenderTarget2D(game.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, game.GraphicsDevice.DisplayMode.Format);
            reflectionRenderTarget = new RenderTarget2D(game.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, game.GraphicsDevice.DisplayMode.Format);

            Effect effect = game.Content.Load<Effect>("Effects/Water");
            VertexDeclaration declaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionTexture.VertexElements);
            Height = scale.Z;

            VertexPositionNormalColorTexture[] vertexes = CreateMesh(scale.X / 2f, scale.Y / 2f, scale.Z / 2f);
            int[] indices = CreateIndices(vertexes);

            CustomMesh custommesh = new CustomMesh(
                game.GraphicsDevice,
                vertexes,
                indices,
                declaration);
            mesh = new WaterSurface(game, custommesh, effect);
                
            //device, CreateMesh(scale.X / 2f, scale.Y / 2f, scale.Z / 2f), effect, declaration);
            mesh.WaterBumpMap = game.Content.Load<Texture2D>(folder + "waterbump");

            mesh.Visibility = Visibility.Standard;
        }
        public void Dispose()
        {
            //mesh.Visible = false;
            mesh = null;
            refractionRenderTarget = null;
            reflectionRenderTarget = null;
        }

        VertexPositionNormalColorTexture[] CreateMesh(float width, float length, float height)
        {
            VertexPositionNormalColorTexture[] waterVertices = new VertexPositionNormalColorTexture[6];

            for (int i = 0; i < waterVertices.Length; i++)
            {
                waterVertices[i] = new VertexPositionNormalColorTexture();
                waterVertices[i].Normal = Vector3.Up;
                waterVertices[i].Color = Color.White.ToVector4();
            }

            waterVertices[0].Position = new Vector3(width, length, height);
            waterVertices[0].TextureCoordinate = new Vector2(0, 1);

            waterVertices[2].Position = new Vector3(-width, -length, height);
            waterVertices[2].TextureCoordinate = new Vector2(1, 0);

            waterVertices[1].Position = new Vector3(width, -length, height);
            waterVertices[1].TextureCoordinate = new Vector2(0, 0);

            waterVertices[3].Position = new Vector3(width, length, height);
            waterVertices[3].TextureCoordinate = new Vector2(0, 1);

            waterVertices[5].Position = new Vector3(-width, length, height);
            waterVertices[5].TextureCoordinate = new Vector2(1, 1);

            waterVertices[4].Position = new Vector3(-width, -length, height);
            waterVertices[4].TextureCoordinate = new Vector2(1, 0);

            return waterVertices;
        }

        int[] CreateIndices(VertexPositionNormalColorTexture[] vertices)
        {
            int[] indices = new int[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                indices[i] = i;
            }

            return indices;
        }

        public Plane GetWaterPlane()
        {
            return new Plane(new Vector4(0f, 0f, -1f, Height));
        }

        Plane CreatePlane(Vector4 plane, Camera camera, bool flipclip)
        {
            if (flipclip) plane *= -1f;

            Matrix worldViewProjection = camera.View * camera.Projection;
            Matrix inverseWorldViewProjection = Matrix.Invert(worldViewProjection);
            inverseWorldViewProjection = Matrix.Transpose(inverseWorldViewProjection);

            Vector4 newplane = Vector4.Transform(plane, inverseWorldViewProjection);
            return new Plane(newplane);
        }

        void SetTarget(GraphicsDevice device, Camera camera, RenderTarget2D target, DrawPlane type)
        {
            float heightadjust = type == DrawPlane.Above ? 1f : -1f;
            Plane plane = CreatePlane(new Vector4(0f, 0f, -1f, Height + heightadjust), 
                camera, type == DrawPlane.Above ? true : false);

            device.ClipPlanes[0].Plane = plane;
            device.ClipPlanes[0].IsEnabled = true;
            device.SetRenderTarget(0, target);

            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, 
                Color.Black, 1.0f, 0);
        }

        Texture2D GetTexture(GraphicsDevice device, RenderTarget2D rendertarget)
        {
            device.ClipPlanes[0].IsEnabled = false;
            device.SetRenderTarget(0, null);
            return rendertarget.GetTexture();
        }
        Sprite2D testsprite = null;
        public void OpenRefractionTarget(GraphicsDevice device, Camera camera)
        {
            if (Visible)
            {
                SetTarget(device, camera, refractionRenderTarget, DrawPlane.Below);
            }
        }

        public void CloseRefractionTarget(GraphicsDevice device)
        {
            if (Visible)
            {
                mesh.RefractionMap = GetTexture(device, refractionRenderTarget);
                if (testsprite != null) testsprite = null;
                testsprite = new Sprite2D(new AnimatedTexture(mesh.RefractionMap));
                testsprite.Visible = true;
            }
        }

        public void OpenReflectionTarget(GraphicsDevice device, Camera camera)
        {
            if (Visible)
            {
                SetTarget(device, camera, reflectionRenderTarget, DrawPlane.Above);
                mesh.ReflectionView = camera.View;
            }
        }

        public void CloseReflectionTarget(GraphicsDevice device)
        {
            if (Visible)
            {
                mesh.ReflectionMap = GetTexture(device, reflectionRenderTarget);
            }
        }

        public void Update(Time elapsed)
        {
        }
    }*/
}