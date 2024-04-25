using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ChiseDrive.Graphics
{
    public class SpriteBar3D
    {
        public enum Direction
        {
            Horizontal,
            Vertical
        };

        Sprite3D filled;
        Sprite3D unfilled;

        Direction direction;

        Vector3 position;
        float length;
        float height;
        float width;
        float percent;

        public Vector4 LocationPercent
        {
            set
            {
                position.X = value.X;
                position.Y = value.Y;
                position.Z = value.Z;
                percent = value.W;
            }
        }

        public Vector3 Location
        {
            set
            {
                position = value;
            }
        }
        public float Length
        {
            set
            {
                length = value;
            }
        }
        public float Percent
        {
            set
            {
                percent = value;
            }
        }

        float scale;
        public float Scale
        {
            set
            {
                scale = value;
                filled.Scale = value;
                unfilled.Scale = value;
            }
        }

        /// <summary>
        /// Creates a SpriteBar2D, requires images: filenameCap0, filenameCap1, filenameFilled, filenameUnfilled, filenameValue.
        /// </summary>
        /// <param name="content">Content Manager</param>
        /// <param name="texturefilename">Base Filename</param>
        /// <param name="fontfilename">Font to Use</param>
        /// <param name="label">Text Description of the Bar</param>
        /// <param name="direction">Direction</param>
        public SpriteBar3D(ContentManager content, float width, float height, string texturefilename, Direction direction)
        {
            AnimatedTexture texture = new AnimatedTexture(content, texturefilename + "Filled");
            float widthscale = (float)texture.Frame.Width / (float)texture.Frame.Height;
            filled = new Sprite3D(Vector3.Zero, // Position
                new Vector3(0f, height, 0f), // Up
                new Vector3(-height * widthscale, 0f, 0f),  // Left
                1f, // Scale
                Color.White, // Color 
                texture);
            this.width = widthscale * height;

            texture = new AnimatedTexture(content, texturefilename + "Unfilled");
            widthscale = (float)texture.Frame.Width / (float)texture.Frame.Height;
            unfilled = new Sprite3D(Vector3.Zero, // Position
                new Vector3(0f, height, 0f), // Up
                new Vector3(-height * widthscale, 0f, 0f),  // Left
                1f, // Scale
                Color.White, // Color 
                texture);

            this.height = height;
            this.length = width;
            this.direction = direction;
            Scale = 1f;
        }

        public bool Visible
        {
            get
            {
                return unfilled.Visible;
            }
            set
            {
                filled.Visible = value;
                unfilled.Visible = value;
            }
        }

        public Color Color
        {
            set
            {
                filled.Color = value;
                unfilled.Color = value;
            }
        }

        public float Fade
        {
            set
            {
                Color color = filled.Color;
                color.A = (byte)(255f * value);

                this.Color = color;
            }
        }

        public void Dispose()
        {
            filled.Dispose();
            unfilled.Dispose();
        }

        public void Rebuild()
        {
            if (direction == Direction.Horizontal)
            {
                unfilled.Position = position;
                filled.Position = position;

                Vector3 CapOffset = new Vector3(width * scale, 0f, 0f);

                filled.Position = position - ((1f - percent) * CapOffset);
                filled.LeftUp(new Vector3(-this.width * percent, 0f, 0f), new Vector3(0f, height, 0f));

                unfilled.Position = position + (percent * CapOffset);
                unfilled.LeftUp(new Vector3(-this.width * (1f - percent), 0f, 0f), new Vector3(0f, height, 0f));
            }
        }
    }
}