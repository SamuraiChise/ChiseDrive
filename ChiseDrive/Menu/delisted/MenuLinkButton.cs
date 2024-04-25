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
    class MenuLinkButton : MenuComponent
    {
        public string Font;
        public string Caption;
        public MenuCommand Action;
        public List<MenuLink> Links;
        public bool Selected;
        public bool Enabled;
        
        SpriteFont font;
        
        public MenuLinkButton() 
        {
            Links = new List<MenuLink>();
        }
        
        public override void ResolveAssets(ContentManager content)
        {
            font = content.Load<SpriteFont>(Font);
        }

#if !Xbox
        public override void Write(Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler.ContentWriter output)
        {
            output.Write(Font);
            output.Write(Caption);
            Action.Write(output);
            output.Write(Links.Count);
            for (int i = 0; i < Links.Count; i++)
            {
                Links[i].Write(output);
            }
            output.Write(Selected);
            output.Write(Enabled);
            base.Write(output);
        }
#endif

        public override void Read(ContentReader input)
        {
            Font = input.ReadString();
            Caption = input.ReadString();
            Action = MenuCommand.Read(input);
            for (int linkcount = input.ReadInt32(); linkcount > 0; linkcount--)
            {
                Links.Add(MenuLink.Read(input));
            }
            Selected = input.ReadBoolean();
            Enabled = input.ReadBoolean();
            base.Read(input);
        }

        public override void SetState(MenuComponent.State next)
        {
            if (Selected == true && next == State.Idle) base.SetState(State.Highlight);
            else base.SetState(next);
        }

        public override MenuCommand CheckInput(ChiseDrive.Input.Instruction value)
        {
            if (Selected) // Check for Navigation
            {
                foreach (MenuLink l in Links)
                {
                    if (l.LinkButton == value.Type)
                    {
                        MenuCommand c = new MenuCommand();
                        c.Command = l.LinkName;
                        c.Type = MenuCommand.CommandType.HighlightObject;
                        return c;
                    }
                }

                if (value.Type == "A")
                {
                    return Action;
                }
            }

            return base.CheckInput(value);
        }

        public override void Update(Time elapsed)
        {
            base.Update(elapsed);
        }

        public override void Draw(SpriteBatch batch)
        {/*
            // Text
            Vector2 textpos = new Vector2();
            textpos.X = CurrentKeyFrame.Position.X;
            textpos.Y = CurrentKeyFrame.Position.Y;

            batch.DrawString(font, Caption, textpos, CurrentKeyFrame.Color);

            base.Draw(batch);*/
       // }
    //}*/
}
