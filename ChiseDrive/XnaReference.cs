using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ChiseDrive
{
    public class XnaReference
    {
        ContentManager content;
        GraphicsDevice device;
        SpriteBatch sprites;

        public ContentManager Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
            }
        }
        public GraphicsDevice Device
        {
            get
            {
                return device;
            }
            set
            {
                device = value;
            }
        }
        public SpriteBatch Sprites
        {
            get
            {
                return sprites;
            }
            set
            {
                sprites = value;
            }
        }

        public XnaReference(ContentManager content, GraphicsDevice device, SpriteBatch sprites)
        {
            this.device = device;
            this.content = content;
            this.sprites = sprites;
        }
        public void Dispose()
        {
            content = null;
            device = null;
            sprites = null;
        }
    }
}