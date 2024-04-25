using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChiseDrive.Menu
{
    class MenuImage : IMenuAsset
    {
        public const string TypeIdentifier = "MenuImage";
        static readonly Vector2 Origin = new Vector2(0.5f, 0.5f);

        Texture2D image;

        public string Type { get { return TypeIdentifier; } }
        public string DataTag { get; set; }
        public MenuKeyframe Keyframe { get; set; }

        public void Build(MenuDevice device)
        {
            image = device.ActiveScreen.Assets[DataTag] as Texture2D;
        }

        Rectangle SpriteRectangle(MenuKeyframe keyframe)
        {
            Rectangle drawrect = Rectangle.Empty;

            drawrect.X = (int)(Keyframe.Position.X + keyframe.Position.X);
            drawrect.Y = (int)(Keyframe.Position.Y + keyframe.Position.Y);
            drawrect.Width = (int)((float)image.Width * keyframe.Size.X * Keyframe.Size.X);
            drawrect.Height = (int)((float)image.Height * keyframe.Size.Y * Keyframe.Size.Y);

            return drawrect;
        }

        public void Draw(SpriteBatch sprites, MenuKeyframe keyframe)
        {
            sprites.Draw(
                image, 
                SpriteRectangle(keyframe), 
                null,
                Keyframe.SpriteColor,
                Keyframe.Rotation + keyframe.Rotation,
                Origin, 
                SpriteEffects.None,
                Keyframe.Layer + keyframe.Rotation);
        }

        public override string ToString()
        {
            return "MenuImage [" + DataTag + "]";
        }
    }
}
