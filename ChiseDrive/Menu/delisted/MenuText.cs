using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ChiseDrive.Menu
{/*
    public class MenuText : MenuComponent
    {
        public String Caption;
        public String Font;
        SpriteFont font;

        public MenuText() { }

#if !Xbox
        public override void Write(Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler.ContentWriter output)
        {
            output.Write(Caption);
            output.Write(Font);
            base.Write(output);
        }
#endif

        public override void Read(ContentReader input)
        {
            Caption = input.ReadString();
            Font = input.ReadString();
            base.Read(input);
        }
        
        public override void ResolveAssets(ContentManager content)
        {
            font = content.Load<SpriteFont>(Font);
        }

        public override void Update(Time elapsed)
        {
            base.Update(elapsed);
        }

        public override void Draw(SpriteBatch batch)
        {/*
            if (font == null) return;
            Vector2 textpos = new Vector2();
            textpos.X = CurrentKeyFrame.Position.X;
            textpos.Y = CurrentKeyFrame.Position.Y;
            batch.DrawString(font, Caption, textpos, CurrentKeyFrame.Color);

            base.Draw(batch);*/
       // }
  //  }*/
}