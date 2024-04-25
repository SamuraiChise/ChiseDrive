using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ChiseDrive.Graphics;
using Microsoft.Xna.Framework.Content;
#if !Xbox
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endif

namespace ChiseDrive.World
{
    public class TextureSkybox : IWorldComponent, IDisposable
    {
        string texturename;
        Vector3 position;
        Vector3 lookat;
        Vector3 up;
        Vector2 scale;
        float alpha;

        SkyboxPane pane;
        
        public bool Visible
        {
            get
            {
                return pane.Visible;
            }
            set
            {
                pane.Visible = value;
            }
        }

        public void Initialize(ChiseDriveGame game)
        {
            pane = new SkyboxPane(
                new AnimatedTexture(game.Content, texturename),
                position, lookat, up, scale, alpha);
            pane.Visible = true;
            SkyboxPane.Effect = game.Content.Load<Microsoft.Xna.Framework.Graphics.Effect>("Effects/Skybox");
            SkyboxPane.VertexDeclaration = new Microsoft.Xna.Framework.Graphics.VertexDeclaration(
                game.GraphicsDevice, Microsoft.Xna.Framework.Graphics.VertexPositionTexture.VertexElements);
        }

        public void Dispose()
        {
            pane.Visible = false;
            pane = null;
        }

        /// <summary>
        /// If a World Component does not need to modify height, return the initial
        /// </summary>
        /// <param name="initial">The vector to modify</param>
        /// <returns>A modified height or the initial</returns>
        public Vector3 CorrectForHeight(Vector3 initial) { return initial; }
        public Vector3 CorrectForBounds(Vector3 initial) { return GameWorld.InvalidPosition; }
        public void SizeBounds(ref Rectangle rectangle) { }

        public TextureSkybox() { }
        public TextureSkybox(string texture, Vector3 position, Vector3 lookat, Vector3 up, Vector2 scale, float alpha)
        {
            this.texturename = texture;
            this.position = position;
            this.lookat = lookat;
            this.up = up;
            this.scale = scale;
            this.alpha = alpha;
        }

        public void Update(Time elapsed) { }

#if !Xbox
        public void Write(ContentWriter output)
        {
            output.Write(texturename);
            output.Write(position);
            output.Write(lookat);
            output.Write(up);
            output.Write(scale);
            output.Write(alpha);
        }
#endif
        public void Read(ContentReader input)
        {
            texturename = input.ReadString();
            position = input.ReadVector3();
            lookat = input.ReadVector3();
            up = input.ReadVector3();
            scale = input.ReadVector2();
            alpha = input.ReadSingle();
        }
    }
}