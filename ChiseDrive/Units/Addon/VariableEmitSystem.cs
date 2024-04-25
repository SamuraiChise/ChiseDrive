using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChiseDrive.Graphics;

namespace ChiseDrive.Units
{
    public class VariableEmitSystem : System, ILightEmitter
    {
        public override string Name
        {
            get;
            set;
        }
        static public VariableEmitSystem TryBuildSystem(String name,
            ChiseDrive.Particles.ParticleEmitterSettings emitsettings,
            List<AttachmentPoint> attachments)
        {
            VariableEmitSystem system = new VariableEmitSystem();
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < attachments.Count; j++)
                {
                    string testname = name;
                    if (i > 0) testname += i;

                    if (attachments[j].Name == testname)
                    {
                        system.emitters.Add(new SimpleEmitter(attachments[j], emitsettings));
                    }
                }
            }

            if (system.emitters.Count > 0)
            {
                return system;
            }
            else return null;
        }

        List<SimpleEmitter> emitters = new List<SimpleEmitter>();

        public bool Active
        {
            set
            {
                foreach (SimpleEmitter emitter in emitters)
                {
                    emitter.Active = value;
                }
            }
        }

        public List<PointLight> Lights
        {
            get
            {
                List<PointLight> lights = new List<PointLight>();
                foreach (SimpleEmitter emitter in emitters)
                {
                    lights.AddRange(emitter.Lights);
                }
                return lights;
            }
        }

        public float Throttle
        {
            set
            {
                foreach (SimpleEmitter emitter in emitters)
                {
                    emitter.Percent = value;

                    if (value == 0f)
                    {
                        emitter.Active = false;
                    }
                    else
                    {
                        emitter.Active = true;
                    }
                }
            }
        }

        public override void Dispose()
        {
            foreach (SimpleEmitter emitter in emitters)
            {
                emitter.Dispose();
            }
            emitters.Clear();
        }

        public override void Update(Time elapsed)
        {
            foreach (SimpleEmitter emitter in emitters)
            {
                emitter.Update(elapsed);
            }
        }
    }
}