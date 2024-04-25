using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Graphics;

namespace ChiseDrive.Particles
{
    public class ParticleEmitterSettings
    {
        public String ParticleSystemName;
        public String SoundCueName;

        public float ParticlesPerSecond;
        public float ParticlesPerEmit;
        public float ParticleExitSpeed;

        public float LightRangeMax;
        public float LightRangeMin;
        public float LightFalloff;
        public Color LightColorLow;
        public Color LightColorHigh;
        public float LightFrames;

        public LightScope LightScope;
    }
}