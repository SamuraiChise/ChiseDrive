using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Graphics;

namespace ChiseDrive.Menu
{/*
    struct MenuAsset
    {
        
 

        /// <summary>
        /// The tag used for finding the correct image, can be null.
        /// </summary>
        public string AssetTag { get; set;}
        IAnimated asset = null;

        public void Rebuild(MenuDevice device)
        {
            switch (AssetType)
            {
                case Type.Image: asset = device.ActiveScreen.Assets[AssetTag] as AnimatedTexture; break;
                case Type.Text: asset = device.ActiveScreen.Assets[AssetTag] as AnimatedText; break;
                case Type.Bar: asset = device.ActiveScreen.Assets[AssetTag] as AnimatedBar; break;
                default: asset = null; break;
            }
        }


        public void Update(Time elapsed)
        {
            if (asset != null) asset.Update(elapsed);
        }

        public void Draw(SpriteBatch sprites, MenuKeyframe keyframe)
        {
            switch (AssetType)
            {
                case Type.Image:
                    sprites.Draw(asset.Frame, SpriteRect(keyframe), null, SpriteColor(keyframe), 0f, Vector2.Zero, SpriteEffects.None, Layer + keyframe.Layer);
                    break;
                case Type.Text:
                    Vector2 position = Vector2.Zero;
                    Vector2 size = Vector2.Zero;
                    RebuildFontBounding(keyframe.Position, ref position, ref size);
                    sprites.DrawString(asset.Font, asset.Text, position, SpriteColor(keyframe.Color), 0f, Vector2.Zero, size, SpriteEffects.None, Layer + keyframe.Layer);
                    break;
                default: break;
            }
        }
    }*/
}