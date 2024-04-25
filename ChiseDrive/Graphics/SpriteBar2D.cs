using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ChiseDrive.Graphics
{
    public class SpriteBar2D : Drawable2D
    {
        public enum Direction
        {
            Horizontal,
            Vertical
        };

        Sprite2D cap0;
        Sprite2D cap1;
        Sprite2D filled;
        Sprite2D unfilled;
        Sprite2D slider;

        Direction direction;

        float length;
        float percent;
        float target;
        float fillSpeed = 0.02f;
        public float FillSpeed
        {
            set
            {
                fillSpeed = value;
            }
        }
        public override Vector2 Center
        {
            set
            {
                position = value;
                Rebuild();
            }
        }
        public float Length
        {
            set
            {
                length = value;
                Rebuild();
            }
        }
        public float PercentTarget
        {
            set
            {
                target = value;
            }
        }
        public float Percent
        {
            set
            {
                percent = target = value;
                Rebuild();
            }
        }
        public override float Layer
        {
            set
            {
                cap0.Layer = value;
                cap1.Layer = value;
                filled.Layer = value;
                unfilled.Layer = value;
                slider.Layer = value;
            }
        }

        public Rectangle DrawRectangle
        {
            get
            {
                Rectangle drawrect = new Rectangle();
                drawrect = cap0.DrawRectangle;

                if (direction == Direction.Horizontal)
                {
                    drawrect.Width = cap0.DrawRectangle.Width
                        + cap1.DrawRectangle.Width
                        + unfilled.DrawRectangle.Width;
                    drawrect.Height = unfilled.DrawRectangle.Height;
                }
                else
                {
                    drawrect.Width = unfilled.DrawRectangle.Width;
                    drawrect.Height = cap0.DrawRectangle.Height
                        + cap1.DrawRectangle.Height
                        + unfilled.DrawRectangle.Height;
                }

                return drawrect;
            }
        }

        public override Vector2 Scale
        {
            set
            {
                cap0.Scale = value;
                cap1.Scale = value;
                filled.Scale = value;
                unfilled.Scale = value;
                slider.Scale = value;
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
        public SpriteBar2D(ContentManager content, string texturefilename, Direction direction)
        {
            cap0 = new Sprite2D(content, texturefilename + "Cap0");
            cap1 = new Sprite2D(content, texturefilename + "Cap1");
            filled = new Sprite2D(content, texturefilename + "Filled");
            unfilled = new Sprite2D(content, texturefilename + "Unfilled");
            slider = new Sprite2D(content, texturefilename + "Value");

            this.direction = direction;
        }

        public override bool Visible
        {
            get
            {
                return cap0.Visible;
            }
            set
            {
                cap0.Visible = value;
                cap1.Visible = value;
                filled.Visible = value;
                unfilled.Visible = value;
                slider.Visible = value;
            }
        }

        public override Color Color
        {
            set
            {
                cap0.Color = value;
                cap1.Color = value;
                filled.Color = value;
                unfilled.Color = value;
                slider.Color = value;
            }
        }

        /// <summary>
        /// Fades the Sprite out.
        /// </summary>
        /// <param name="fadetime">Time to Fade</param>
        public override void FadeOut(Time fadetime)
        {
            cap0.FadeOut(fadetime);
            cap1.FadeOut(fadetime);
            filled.FadeOut(fadetime);
            unfilled.FadeOut(fadetime);
            slider.FadeOut(fadetime);
        }

        /// <summary>
        /// Fades the Sprite in.
        /// </summary>
        /// <param name="fadespeed">Time to Fade</param>
        public override void FadeIn(Time fadetime)
        {
            cap0.FadeIn(fadetime);
            cap1.FadeIn(fadetime);
            filled.FadeIn(fadetime);
            unfilled.FadeIn(fadetime);
            slider.FadeIn(fadetime);
        }

        public override void Update(Time elapsed)
        {
            if (percent != target)
            {
                percent = Helper.EaseTo(percent, target, fillSpeed, elapsed);
                Rebuild();
            }
            cap0.Update(elapsed);
            cap1.Update(elapsed);
            filled.Update(elapsed);
            unfilled.Update(elapsed);
            slider.Update(elapsed);
        }

        public new void Dispose()
        {
            cap0.Dispose();
            cap1.Dispose();
            filled.Dispose();
            unfilled.Dispose();
            slider.Dispose();
        }

        void Rebuild()
        {
            if (direction == Direction.Horizontal)
            {
                int barlength = (int)length - cap0.Texture.Frame.Width - cap1.Texture.Frame.Width;
                int halflength = barlength / 2;

                cap0.Center = new Vector2(position.X - halflength - (cap0.Texture.Frame.Width / 2), position.Y);
                cap1.Center = new Vector2(position.X + halflength + (cap1.Texture.Frame.Width / 2), position.Y);

                unfilled.Center = position;
                Rectangle fullrect = unfilled.DrawRectangle;
                fullrect.Width = barlength;
                fullrect.X = (int)position.X - halflength;

                Rectangle unfilledrect = fullrect;
                Rectangle filledrect = fullrect;
                Rectangle sliderrect = fullrect;

                int sliderWidth = slider.Texture.Frame.Width;

                if (percent == 0f)
                {
                    filledrect.Width = 0;
                    sliderrect.Width = 0;
                }
                else if (percent == 1f)
                {
                    unfilledrect.Width = 0;
                    sliderrect.Width = 0;
                }
                else
                {
                    // The slider moves from fullrect.Y to fullrect.Width - sliderWidth
                    int sliderpoint = 
                        fullrect.X 
                        + (int)((float)(fullrect.Width - sliderWidth) * (percent));

                    sliderrect.Width = sliderWidth;
                    sliderrect.X = sliderpoint;

                    // The unfilled is from fullrect.Y to sliderpoint
                    filledrect.X = fullrect.X;
                    filledrect.Width = sliderpoint - fullrect.X;

                    // The filled rect is from sliderWidth + sliderpoint to fullrect.Width
                    unfilledrect.X = sliderpoint + sliderWidth;
                    unfilledrect.Width = fullrect.Width - (sliderpoint + sliderWidth - fullrect.X);
                }

                unfilled.DrawRectangle = unfilledrect;
                filled.DrawRectangle = filledrect;
                slider.DrawRectangle = sliderrect;
            }

            if (direction == Direction.Vertical)
            {
                int barlength = (int)length - cap0.Texture.Frame.Height - cap1.Texture.Frame.Height;
                int halflength = barlength / 2;

                cap0.Center = new Vector2(position.X, position.Y - halflength - cap0.Texture.Frame.Height / 2);
                cap1.Center = new Vector2(position.X, position.Y + halflength + cap0.Texture.Frame.Height / 2);

                unfilled.Center = position;
                Rectangle fullrect = unfilled.DrawRectangle;
                fullrect.Height = barlength;
                fullrect.Y = (int)position.Y - halflength;

                Rectangle unfilledrect = fullrect;
                Rectangle filledrect = fullrect;
                Rectangle sliderrect = fullrect;

                int sliderheight = slider.Texture.Frame.Height;

                if (percent == 0f)
                {
                    filledrect.Height = 0;
                    sliderrect.Height = 0;
                }
                else if (percent == 1f)
                {
                    unfilledrect.Height = 0;
                    sliderrect.Height = 0;
                }
                else
                {
                    // The slider moves from fullrect.Y to fullrect.Height - sliderheight
                    int sliderpoint = 
                        fullrect.Y 
                        + (int)((float)(fullrect.Height - sliderheight) * (1f - percent));

                    sliderrect.Height = sliderheight;
                    sliderrect.Y = sliderpoint;

                    // The unfilled is from fullrect.Y to sliderpoint
                    unfilledrect.Y = fullrect.Y;
                    unfilledrect.Height = sliderpoint - fullrect.Y;

                    // The filled rect is from sliderheight + sliderpoint to fullrect.Height
                    filledrect.Y = sliderpoint + sliderheight;
                    filledrect.Height = fullrect.Height - (sliderpoint + sliderheight - fullrect.Y);
                }

                unfilled.DrawRectangle = unfilledrect;
                filled.DrawRectangle = filledrect;
                slider.DrawRectangle = sliderrect;
            }
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            // DO NOTHING! (each of the objects for this are already Drawable2D's and will
            // hve their own respective draws called by the masterbatcher.
        }
    }
}