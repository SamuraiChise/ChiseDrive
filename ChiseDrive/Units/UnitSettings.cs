using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#if !Xbox
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endif

namespace ChiseDrive.Units
{
    public class UnitSettings
    {
        public class GameModelSettings
        {
            public String MeshName;
            public String Material;
            public String Effect;
            public float Scale;


#if !XBOX
            public void Write(ContentWriter output)
            {
                output.Write(Scale);
                output.Write(MeshName);
                output.Write(Material);
                output.Write(Effect);
            }
#endif
            public void Read(ContentReader input)
            {
                Scale = input.ReadSingle();
                MeshName = input.ReadString();
                Material = input.ReadString();
                Effect = input.ReadString();
            }
        };
        public class AddonSettings
        {
            public GameModelSettings AddonBody = new GameModelSettings();
            public string AddonSystemName;
            public string EmitterSettingsName;
            public string ParentBoneName;
            public string BodyBoneName;
            public float Cooldown;
            public int Ammo;

#if !Xbox
            public void Write(ContentWriter output)
            {
                output.Write(AddonBody.Scale);
                if (AddonBody.Scale > 0f) AddonBody.Write(output);

                output.Write(AddonSystemName);
                output.Write(EmitterSettingsName);
                output.Write(ParentBoneName);
                output.Write(BodyBoneName);
                output.Write(Cooldown);
                output.Write(Ammo);
            }
#endif

            public void Read(ContentReader input)
            {
                float scale = input.ReadSingle();
                if (scale > 0f)
                {
                    AddonBody = new GameModelSettings();
                    AddonBody.Read(input);
                }
                else
                {
                    AddonBody = null;
                }

                AddonSystemName = input.ReadString();
                EmitterSettingsName = input.ReadString();
                ParentBoneName = input.ReadString();
                BodyBoneName = input.ReadString();

                Cooldown = input.ReadSingle();
                Ammo = input.ReadInt32();
            }
        };

        public GameModelSettings UnitModel = new GameModelSettings();
        public List<AddonSettings> Addons = new List<AddonSettings>();

        //public string LocomotionStyle;
        public float TurnRate;
        public float Acceleration;
        public float Deceleration;
        public float MaxSpeed;
        public float CollisionRadius;
        public float AttackRange;

#if !XBOX
        public void Write(ContentWriter output)
        {
            output.Write(UnitModel.Scale);
            if (UnitModel.Scale > 0f) UnitModel.Write(output);

            output.Write(Addons.Count);
            foreach (AddonSettings a in Addons)
            {
                a.Write(output);
            }

            //output.Write(LocomotionStyle);
            output.Write(TurnRate);
            output.Write(Acceleration);
            output.Write(Deceleration);
            output.Write(MaxSpeed);
            output.Write(CollisionRadius);
            output.Write(AttackRange);
        }
#endif
        public void Read(ContentReader input)
        {
            float scale = input.ReadSingle();
            if (scale > 0f)
            {
                UnitModel.Read(input);
            }
            else
            {
                UnitModel = null;
            }

            int count = input.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                AddonSettings a = new AddonSettings();
                a.Read(input);
                Addons.Add(a);
            }

            //LocomotionStyle = input.ReadString();
            TurnRate = input.ReadSingle();
            Acceleration = input.ReadSingle();
            Deceleration = input.ReadSingle();
            MaxSpeed = input.ReadSingle();
            CollisionRadius = input.ReadSingle();
            AttackRange = input.ReadSingle();
        }
    };
}
