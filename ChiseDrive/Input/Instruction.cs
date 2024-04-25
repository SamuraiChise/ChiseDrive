using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace ChiseDrive.Input
{
    /// <summary>
    /// These are AI/USER commands that can be issued to units.
    /// Don't pass single commands around, instead use the CommandStack
    /// because it can recurse through lots of different commands.
    /// </summary>
    public struct Instruction
    {
        String type;
        public String Type { get { return type; } }
        Vector2 data;
        public Vector2 Data { get { return data; } }

        public static readonly Instruction Empty = new Instruction("");

        public Instruction(String t)
        {
            type = t;
            data = Vector2.Zero;
        }

        public Instruction(String t, Vector2 data)
        {
            type = t;
            this.data = data;
        }

        public static bool operator !=(Instruction lhs, Instruction rhs)
        {
            if (lhs.data == rhs.data && lhs.type == rhs.type) return false;
            return true;
        }

        public static bool operator ==(Instruction lhs, Instruction rhs)
        {
            if (lhs.data != rhs.data || lhs.type != rhs.type) return false;
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
    }
}
