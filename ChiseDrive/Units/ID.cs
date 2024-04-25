using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Units
{
    public struct ID
    {
        int value;
        public int Value
        {
            get
            {
                return value;
            }
        }

        static int lastID = 0;

        public static ID None = ID.Generate();

        static public ID Generate()
        {
            ID value; value.value = ++lastID; return value;
        }

        public static bool operator ==(ID lhs, ID rhs)
        {
            return lhs.Value == rhs.Value;
        }

        public static bool operator !=(ID lhs, ID rhs)
        {
            return lhs.Value != rhs.Value;
        }

        public override string ToString()
        {
            return "ID: " + value;
        }
    }
}