using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChiseDrive.Graphics;

namespace ChiseDrive.Units
{
    public class AccentLight : Addon, ILightEmitter
    {
        PointLight light;

        public List<PointLight> Lights
        {
            get
            {
                List<PointLight> retvalue = new List<PointLight>();
                retvalue.Add(light);
                return retvalue;
            }
        }

        public AccentLight(PointLight light, AttachmentPoint root)
            : base(root)
        {
            this.light = light;
        }

        public override void Update(Time elapsed)
        {
            base.Update(elapsed);

            this.light.Position = new Microsoft.Xna.Framework.Vector4(Position, 1f);
        }
    }
}