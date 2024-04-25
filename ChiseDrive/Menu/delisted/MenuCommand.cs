using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ChiseDrive.Input;

namespace ChiseDrive.Menu
{
    public struct MenuCommand
    {
        public enum CommandType
        {
            None,
            OpenScreen,
            HighlightObject,
            External
        };

        public CommandType Type;
        public String Command;
        string data;

        public static readonly MenuCommand Empty = new MenuCommand();

        public static bool operator !=(MenuCommand lhs, MenuCommand rhs)
        {
            if (lhs.Type == rhs.Type && lhs.Command == rhs.Command) return false;
            return true;
        }
        public static bool operator ==(MenuCommand lhs, MenuCommand rhs)
        {
            if (lhs.Type != rhs.Type || lhs.Command != rhs.Command || lhs.data != rhs.data) return false;
            return true;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

#if !Xbox
        public void Write(Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler.ContentWriter output)
        {
            output.Write(Type.ToString());
            output.Write(Command);
        }
#endif

        public static MenuCommand Read(Microsoft.Xna.Framework.Content.ContentReader input)
        {
            MenuCommand m = new MenuCommand();
            m.Type = (MenuCommand.CommandType)Enum.Parse(typeof(MenuCommand.CommandType), input.ReadString(), true);
            m.Command = input.ReadString();
            //m.data = null;
            return m;
        }

        public void SetData(object o)
        {
            //data = o;
        }

        public Instruction AsInstruction()
        {
            //if (data != null) return new Instruction(Command, data);
            return new Instruction(Command);
        }
    }
}