using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace ChiseDrive.Menu
{
    public interface IMenuAsset
    {
        string Type { get; }
        string DataTag { get; set; }
        MenuKeyframe Keyframe { get; set; }
        void Build(MenuDevice device);
        void Draw(SpriteBatch sprites, MenuKeyframe keyframe);
    }
}
