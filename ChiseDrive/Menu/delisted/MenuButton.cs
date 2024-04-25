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
    public class MenuButton : MenuComponent
    {
        public String Button;
        public String Texture;
        public String Font;
        public String Caption;
        public MenuCommand Action;

        AnimatedTexture image;
        SpriteFont font;

        public MenuButton() { }
        
        public override void ResolveAssets(ContentManager content)
        {
            image = new AnimatedTexture(content, Texture);
            font = content.Load<SpriteFont>(Font);
        }

#if !Xbox
        public override void Write(Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler.ContentWriter output)
        {
            output.Write(Button);
            output.Write(Texture);
            output.Write(Font);
            output.Write(Caption);
            Action.Write(output);
            base.Write(output);
        }
#endif

        public override void Read(ContentReader input)
        {
            this.Button = input.ReadString();
            this.Texture = input.ReadString();
            this.Font = input.ReadString();
            this.Caption = input.ReadString();
            this.Action = MenuCommand.Read(input);
            base.Read(input);
        }

        public override MenuCommand CheckInput(ChiseDrive.Input.Instruction value)
        {
            if (Button == value.Type)
            {
                return Action;
            }
            return base.CheckInput(value);
        }

        public override void Update(Time elapsed)
        {
            if (image != null) image.Animate(elapsed);
            base.Update(elapsed);
        }

        public override void Draw(SpriteBatch batch)
        {/*
            if (image == null) return;
            // Button
            Rectangle imagerect = CurrentKeyFrame.Position;
            imagerect.Width = image.Frame.Width;
            imagerect.Height = image.Frame.Height;
            if (image != null) batch.Draw(image.Frame, imagerect, CurrentKeyFrame.Color);

            // Text
            Vector2 textpos = new Vector2();
            textpos.X = imagerect.X + imagerect.Width;
            textpos.Y = imagerect.Y;
            batch.DrawString(font, Caption, textpos, CurrentKeyFrame.Color);

            base.Draw(batch);*/
      //  }
   // }
}