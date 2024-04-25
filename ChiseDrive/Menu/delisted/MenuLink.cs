using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Menu
{
    class MenuLink
    {
        public string LinkName;
        public string LinkButton;

#if !Xbox
        public void Write(Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler.ContentWriter output)
        {
            output.Write(LinkName);
            output.Write(LinkButton);
        }
#endif

        public static MenuLink Read(Microsoft.Xna.Framework.Content.ContentReader input)
        {
            MenuLink m = new MenuLink();

            m.LinkName = input.ReadString();
            m.LinkButton = input.ReadString();

            return m;
        }
    }
}
