using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ChiseDrive.Menu;

// TODO: replace this with the type you want to write out.
using TWrite = ChiseDrive.Menu.MenuScreen;

namespace ChisePipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class MenuWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write(ContentWriter output, TWrite value)
        {
            output.Write(value.Name);
            output.Write(value.Components.Count);
            foreach (MenuComponent component in value.Components)
            {
                output.WriteObject<List<MenuKeyframe>[]>(component.Keyframes);
                //output.WriteObject<List<IMenuAsset>>(component.Assets);
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "ChiseDrive.Menu.MenuReader, ChiseDrive";
        }
    }
}
