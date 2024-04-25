using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ChiseDrive.Menu
{
    public class MenuReader : ContentTypeReader<MenuScreen>
    {
        protected override MenuScreen Read(ContentReader input, MenuScreen existingInstance)
        {
            MenuScreen screen = new MenuScreen();
            screen.Name = input.ReadString();

            int count = input.ReadInt32();

            while (count > 0)
            {
                MenuComponent component = new MenuComponent();
                component.Keyframes = input.ReadObject<List<MenuKeyframe>[]>();
                component.Assets = input.ReadObject<List<IMenuAsset>>();
                screen.Components.Add(component);
            }

            return screen;
        }
    }
}