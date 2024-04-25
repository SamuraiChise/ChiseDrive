using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ChiseDrive.Units
{
    public class UnitReader : ContentTypeReader<UnitSettings>
    {
        protected override UnitSettings Read(ContentReader input, UnitSettings existingInstance)
        {
            UnitSettings newsettings = new UnitSettings();
            newsettings.Read(input);
            return newsettings;
        }
    }
}