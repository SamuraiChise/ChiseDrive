using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Menu
{/*
    public class MenuConditionalButton : MenuButton
    {
        public String ConditionKey;
        public String DefaultCaption;
        public String ConditionalCaption;
        public MenuCommand DefaultCommand;
        public MenuCommand ConditionalCommand;

        public MenuConditionalButton()
        {
        }

#if !Xbox
        public override void Write(Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler.ContentWriter output)
        {
            output.Write(ConditionKey);
            output.Write(DefaultCaption);
            output.Write(ConditionalCaption);
            DefaultCommand.Write(output);
            ConditionalCommand.Write(output);
            base.Write(output);
        }
#endif

        public override void Read(Microsoft.Xna.Framework.Content.ContentReader input)
        {
            ConditionKey = input.ReadString();
            DefaultCaption = input.ReadString();
            ConditionalCaption = input.ReadString();
            DefaultCommand = MenuCommand.Read(input);
            ConditionalCommand = MenuCommand.Read(input);
            base.Read(input);
        }

        public void SetCondition(bool value)
        {
            if (value)
            {
                Caption = ConditionalCaption;
                Action = ConditionalCommand;
            }
            else
            {
                Caption = DefaultCaption;
                Action = DefaultCommand;
            }
        }
    }*/
}