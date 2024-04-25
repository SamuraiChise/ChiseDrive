using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChiseDrive.Particles;
using ChiseDrive.Graphics;

namespace ChiseDrive.Units
{
    public class SimpleEmitter : Addon, ILightEmitter
    {
        protected ParticleEmitter emitter;

        public List<PointLight> Lights
        {
            get
            {
                return emitter.Lights;
            }
        }

        public SimpleEmitter(AttachmentPoint attachment, ParticleEmitterSettings emitsettings)
            : base(attachment)
        {
            if (emitsettings != null) emitter = new ParticleEmitter(this, emitsettings);
            else emitter = null;
        }

        public bool Active
        {
            set
            {
                if (value) emitter.EmitLoop();
                else emitter.StopLoop();
            }
        }

        public float Percent
        {
            set
            {
                emitter.EmitPercent = value;
            }
        }

        public override void Update(Time elapsed)
        {
            base.Update(elapsed);
            if (emitter != null) emitter.Update(elapsed);
        }
    }
}