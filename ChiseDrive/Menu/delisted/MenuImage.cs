using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ChiseDrive.Graphics;

namespace ChiseDrive.Menu
{/*
    public class MenuImage : MenuComponent
    {
        public String Image;

        AnimatedTexture texture;

        public MenuImage() {}

#if !Xbox
        public override void Write(Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler.ContentWriter output)
        {
            output.Write(Image);
            base.Write(output);
        }
#endif

        public override void Read(ContentReader input)
        {
            Image = input.ReadString();
            base.Read(input);
        }

        public override void ResolveAssets(ContentManager content)
        {
            texture = new AnimatedTexture(content, Image);
        }

        public override void Update(Time elapsed)
        {
            if (texture != null) texture.Animate(elapsed);
            base.Update(elapsed);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
        {/*
            Rectangle imagerect = CurrentKeyFrame.Position;
            if (texture != null) batch.Draw(texture.Frame, imagerect, CurrentKeyFrame.Color);

            base.Draw(batch);*/
      //  }
    //}
}