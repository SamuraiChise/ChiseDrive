using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ChiseDrive.Graphics
{
    /// <summary>
    /// A 2D sprite used for drawing in screen coordinates.
    /// Uses Back to Front sorting and Alpha Blending.
    /// </summary>
    public class Sprite2D : Drawable2D, IDisposable
    {
        AnimatedTexture texture;

        Rectangle sourcerect = Rectangle.Empty;
        Rectangle drawrect = Rectangle.Empty;
        Rectangle offsetrect = Rectangle.Empty;
        Rectangle constraint = new Rectangle(0, 0, 1280, 720);

        /// <summary>
        /// The centerpoint for the sprite. (default 0,0)
        /// </summary>
        public override Vector2 Center
        {
            set
            {
                base.Center = value;
                RebuildDrawRect();
            }
            get
            {
                Vector2 center = new Vector2();
                center.X = (float)(drawrect.X + (drawrect.Width / 2));
                center.Y = (float)(drawrect.Y + (drawrect.Height / 2));
                return center;
            }
        }

        /// <summary>
        /// Sets the X/Y scale (default 1,1)
        /// </summary>
        public override Vector2 Scale
        {
            set
            {
                base.Scale = value;
                RebuildDrawRect();
            }
        }

        public Rectangle SourceRectangle
        {
            get
            {
                return sourcerect;
            }
        }

        public Rectangle DrawRectangle
        {
            get
            {
                return drawrect;
            }
            set
            {
                drawrect = value;
                RebuildSourceRect();
            }
        }

        /// <summary>
        /// This is a rectangle for pushing in on all sides.
        /// Valid numbers are:
        /// X 0, Width
        /// Y 0, Width
        /// Width 0, -Width
        /// Height 0, -Height
        /// </summary>
        public Rectangle OffsetRectangle
        {
            get
            {
                return offsetrect;
            }
            set
            {
                offsetrect = value;
                RebuildDrawRect();
            }
        }

        /// <summary>
        /// This is the assumed screen (default 0,0,1280,720)
        /// </summary>
        public Rectangle Screen
        {
            set
            {
                constraint = value;
            }
        }

        public AnimatedTexture Texture
        {
            get
            {
                return texture;
            }
        }

        public Sprite2D(AnimatedTexture texture)
        {
            this.texture = texture;
            RebuildDrawRect();
        }

        public Sprite2D(ContentManager content, string filename)
        {
            this.layer = Drawable2D.BottomLayer;
            this.texture = new AnimatedTexture(content, filename);
            
            RebuildDrawRect();
        }

        public Sprite2D(ContentManager content, string filename, int numberframes)
        {
            this.layer = Drawable2D.BottomLayer;
            this.texture = new AnimatedTexture(content, filename, numberframes);
            
            RebuildDrawRect();
        }

        public void Dispose()
        {
            Visible = false;
            texture = null;
        }

        public void PlayOnce()
        {
            texture.Play();
        }

        public void PlayLoop()
        {
            texture.Loop();
        }

        protected virtual void RebuildSourceRect()
        {
            sourcerect.X = 0;
            sourcerect.Y = 0;
            sourcerect.Width = texture.Frame.Width;
            sourcerect.Height = texture.Frame.Height;
        }

        protected virtual void RebuildDrawRect()
        {
            float width = (float)texture.Frame.Width * scale.X;
            float height = (float)texture.Frame.Height * scale.Y;

            drawrect.Width = (int)width;
            drawrect.Height = (int)height;

            drawrect.X = (int)(position.X - (width / 2f));
            drawrect.Y = (int)(position.Y - (height / 2f));

            //drawrect.X += offsetrect.X;
            //drawrect.Y += offsetrect.Y;
            //drawrect.Width += offsetrect.Width;
            //drawrect.Height += offsetrect.Height;

            RebuildSourceRect();

            //Helper.Constrain(ref sourcerect, ref drawrect, constraint);
        }

        public override void Update(Time elapsed)
        {
            //Debug.DebugText.Write("DrawRect: " + drawrect.X + ", " + drawrect.Y + ", " + drawrect.Width + ", " + drawrect.Height);

            texture.Animate(elapsed);
            base.Update(elapsed);
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            Rectangle draw = drawrect;
            draw.X += offsetrect.X;
            draw.Y += offsetrect.Y;
            draw.Width += offsetrect.Width;
            draw.Height += offsetrect.Height;

            Rectangle source = sourcerect;
            source.X += offsetrect.X;
            source.Y += offsetrect.Y;
            source.Width += offsetrect.Width;
            source.Height += offsetrect.Height;

            spritebatch.Draw(texture.Frame, draw, source, drawcolor, 0f, Vector2.Zero, SpriteEffects.None, layer);
        }
    }
}