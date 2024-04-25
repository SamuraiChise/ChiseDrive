using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#if !Xbox
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endif
using ChiseDrive.Graphics;

namespace ChiseDrive.Units
{
    public class BodySettings
    {
        public String MeshFile;
        public String MaterialFile;
        public String EffectFile;
        public float Scale;

        public LitObject BuildLitObject(Game game)
        {
            Material material = new Material(game.Content, MaterialFile);
            Effect effect = game.Content.Load<Effect>(EffectFile);
            GameModel mesh = new GameModel(game.Content.Load<Model>(MeshFile), Scale);

            LitObject retvalue = new LitObject(game, mesh, material, effect);
            retvalue.Visibility = Visibility.Standard;

            return retvalue;
        }

#if !Xbox
        public void Write(ContentWriter output)
        {
            output.Write(MeshFile);
            output.Write(MaterialFile);
            output.Write(EffectFile);
            output.Write(Scale);
        }
#endif
        public void Read(ContentReader input)
        {
            MeshFile = input.ReadString();
            MaterialFile = input.ReadString();
            EffectFile = input.ReadString();
            Scale = input.ReadSingle();
        }
    };

    public class AddonSettings
    {
        public BodySettings AddonBody;
        public string AddonSystemName;
        public string EmitterSettingsName;
        public string ParentBoneName;
        public string BodyBoneName;
        public float Cooldown;
        public int Ammo;
    };

    public class UnitSettingsEX
    {
        public Attributes Attributes;
        public string ActionFile;

        public BodySettings MainBody;
        public List<AddonSettings> Addons = new List<AddonSettings>();
    }
}
