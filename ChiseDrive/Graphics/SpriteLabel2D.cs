using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ChiseDrive.Graphics
{
    public class SpriteLabel2D : Drawable2D, IDisposable
    {
        SpriteFont font;
        String label;
        String safelabel;

        Vector2 size;

        float maxLength = 0f;
        public float MaxLength
        {
            set
            {
                maxLength = value;
            }
        }
        

        float rotation = 0f;
        bool vertical = false;
        public bool Vertical
        {
            set
            {
                vertical = value;
                if (value)
                {
                    rotation = (3f * (float)Math.PI) / 2f;
                }
                else
                {
                    rotation = 0f;
                }
            }
        }

        Vector2 shadow = Vector2.Zero;
        public Vector2 Shadow
        {
            get { return shadow; }
            set { shadow = value; }
        }
        Color shadowColor = Color.Black;
        public Color ShadowColor
        {
            get { return shadowColor; }
            set { shadowColor = value; }
        }

        public void SetLabelUnsafe(string value)
        {
            label = safelabel = value;
            size = this.font.MeasureString(safelabel);
        }
        public void SetLabelSafe(string value)
        {
            label = value;
            size = this.font.MeasureString(label);

            int length = label.Length;
            bool truncate = false;

            if (maxLength > 0f)
            {
                // Enforcing max length
                float percent = maxLength / size.X;
                if (percent < 1f)
                {
                    length = (int)((float)length * percent);
                    length--;
                    size.X = maxLength;
                    truncate = true;
                }
            }

            safelabel = "";

            for (int i = 0; i < length; i++)
            {
                if (font.Characters.Contains(label[i]) || label[i] == '\n')
                {
                    safelabel += label[i];
                }
                else
                {
                    safelabel += "?";
                }
            }
            if (truncate) safelabel += "..";
        }

        public String Label
        {
            set
            {
                SetLabelUnsafe(value);
            }
            get
            {
                return label;
            }
        }

        public void Recenter()
        {
            size = this.font.MeasureString(safelabel);
            Center = center;
        }

        public Vector2 Size
        {
            get
            {
                return size;
            }
        }

        public override Vector2 Scale
        {
            set
            {
                base.Scale = value;
                Recenter();
            }
        }

        Vector2 center;
        public override Vector2 Center
        {
            set
            {
                center = position = value;

                if (vertical)
                {
                    position.Y += size.X * scale.Y / 2f;
                    position.X -= size.Y * scale.X / 2f;
                }
                else
                {
                    position.X -= size.X * scale.X / 2f;
                    position.Y -= size.Y * scale.Y / 2f;
                }
            }
            get
            {
                Vector2 pos = position;

                if (vertical)
                {
                    pos.Y -= size.X / 2f;
                    pos.X += size.Y / 2f;
                }
                else
                {
                    pos.X += size.X / 2f;
                    pos.Y += size.Y / 2f;
                }

                return pos;
            }
        }

        public SpriteLabel2D(string label, ContentManager content, string font)
        {
            this.font = content.Load<SpriteFont>(font);
            this.layer = Drawable2D.TopLayer;
            Label = label;
        }

        public void Dispose()
        {
            Visible = false;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spritebatch)
        {
            if (shadow != Vector2.Zero)
            {
                spritebatch.DrawString(font, safelabel, position + shadow, shadowColor, rotation, Vector2.Zero, scale, SpriteEffects.None, layer + 0.001f);
            }

            spritebatch.DrawString(font, safelabel, position, drawcolor, rotation, Vector2.Zero, scale, SpriteEffects.None, layer);
        }
    }
}