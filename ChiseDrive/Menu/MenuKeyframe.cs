using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive;

namespace ChiseDrive.Menu
{
    public struct MenuKeyframe
    {
        public static MenuKeyframe None = new MenuKeyframe();

        public Vector2 Position;
        public Vector2 Size;
        public Vector4 Color;
        public float Layer;
        public float Rotation;
        public Time Length;

        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            }
            set
            {
                Position.X = value.X;
                Position.Y = value.Y;
                Size.X = value.Width;
                Size.Y = value.Height;
            }
        }

        public Color SpriteColor
        {
            get
            {
                return new Color(Color.X, Color.Y, Color.Z, Color.W);
            }
        }

        public void FontBounding(ref Rectangle tofit, ref Vector2 position, ref Vector2 size)
        {
            size.X = (float)tofit.Width / size.X;
            size.Y = (float)tofit.Height / size.Y;

            size *= this.Size;

            position.X = (float)tofit.X + Position.X;
            position.Y = (float)tofit.Y + Position.Y;
        }

        public static MenuKeyframe Lerp(MenuKeyframe initial, MenuKeyframe final, float percent)
        {
            MenuKeyframe keyframe = new MenuKeyframe();

            keyframe.Position = Vector2.Lerp(initial.Position, final.Position, percent);
            keyframe.Size = Vector2.Lerp(initial.Size, final.Size, percent);
            keyframe.Color = Vector4.Lerp(initial.Color, final.Color, percent);
            keyframe.Layer = Helper.Lerp(initial.Layer, final.Layer, percent);
            keyframe.Rotation = Helper.Lerp(initial.Rotation, final.Rotation, percent);

            return keyframe;
        }

        public static bool operator ==(MenuKeyframe lhs, MenuKeyframe rhs)
        {
            if (lhs.Position != rhs.Position) return false;
            if (lhs.Size != rhs.Size) return false;
            if (lhs.Color != rhs.Color) return false;
            if (lhs.Layer != rhs.Layer) return false;
            if (lhs.Rotation != rhs.Rotation) return false;
            return true;
        }

        public static bool operator !=(MenuKeyframe lhs, MenuKeyframe rhs)
        {
            if (lhs.Position == rhs.Position) return false;
            if (lhs.Size == rhs.Size) return false;
            if (lhs.Color == rhs.Color) return false;
            if (lhs.Layer == rhs.Layer) return false;
            if (lhs.Rotation == rhs.Rotation) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}