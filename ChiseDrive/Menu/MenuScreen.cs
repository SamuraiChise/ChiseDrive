using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#if !Xbox
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
#endif
using ChiseDrive;
using ChiseDrive.Input;

namespace ChiseDrive.Menu
{
    public class MenuScreen
    {
        string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public List<MenuComponent> Components = new List<MenuComponent>();

        Dictionary<string, string> data = new Dictionary<string, string>();
        public Dictionary<string, string> Data { get { return data; } }

        Dictionary<string, object> assets = new Dictionary<string, object>();
        public Dictionary<string, object> Assets { get { return assets; }}

        public void Open(ContentManager content)
        {
            foreach (MenuComponent component in Components)
            {
                component.SetState(MenuComponent.State.Open);
                component.ResolveAssets(content);
            }
        }

        public void Close()
        {
            foreach (MenuComponent component in Components)
            {
                component.SetState(MenuComponent.State.Close);
            }
        }

        public bool IsClosed()
        {
            bool isclosed = true;
            foreach (MenuComponent component in Components)
            {
                if (!component.IsFinished(MenuComponent.State.Close)) isclosed = false;
            }
            return isclosed;
        }

        public void Update(GameTime gameTime, InstructionStack check, ref List<MenuCommand> commandList)
        {
            while (!check.IsEmpty)
            {
                Instruction inst = check.Pop();

            }

            Time elapsed = Time.FromGameTime(gameTime);

            foreach (MenuComponent component in Components)
            {
                component.Update(elapsed);
            }
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (MenuComponent component in Components)
            {
                component.Draw(batch);
            }
        }
    }
}