using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Menu
{/*
    class MenuLinkToggle : MenuLinkButton, IData
    {
        public List<String> Options;
        public string DataKey { get { return datakey; } set { datakey = value; } }

        string datakey;
        int currentoption = 0;

        public MenuLinkToggle()
        {
            Options = new List<string>();
        }

#if !Xbox
        public override void Write(Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler.ContentWriter output)
        {
            output.Write(Options.Count);
            foreach (string s in Options) output.Write(s);
            output.Write(DataKey);
            base.Write(output);
        }
#endif

        public override void Read(Microsoft.Xna.Framework.Content.ContentReader input)
        {
            for (int count = input.ReadInt32(); count > 0; count--)
            {
                Options.Add(input.ReadString());
            }
            DataKey = input.ReadString();
            base.Read(input);
        }

        public override MenuCommand CheckInput(ChiseDrive.Input.Instruction value)
        {
            if (this.Selected && value.Type == "Left")
            {
                currentoption--;
            }
            if (this.Selected && value.Type == "Right")
            {
                currentoption++;
            }
            currentoption = Helper.Clamp(currentoption, 0, Options.Count);
            return base.CheckInput(value);
        }

        public void WriteData(ref Dictionary<String, String> data)
        {
            if (!data.ContainsKey(DataKey))
            {
                data.Add(DataKey, Options[currentoption]);
            }

            data[DataKey] = Options[currentoption];
        }

        public void ReadData(Dictionary<String, String> data)
        {
            String value;
            data.TryGetValue(DataKey, out value);

            if (value != null)
            {
                for (int i = 0; i < Options.Count; i++)
                {
                    if (Options[i] == value) currentoption = i;
                }
            }
        }
    }*/
}
