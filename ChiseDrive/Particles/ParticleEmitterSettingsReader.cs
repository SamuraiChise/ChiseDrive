using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ChiseDrive.Particles
{
    public class ParticleEmitterSettingsReader : ContentTypeReader<ParticleEmitterSettings>
    {
        protected override ParticleEmitterSettings Read(ContentReader input, ParticleEmitterSettings existingInstance)
        {
            ParticleEmitterSettings pes = new ParticleEmitterSettings();
            
            pes.ParticleSystemName = input.ReadString();
            pes.SoundCueName = input.ReadString();

            pes.ParticlesPerSecond = input.ReadSingle();
            pes.ParticlesPerEmit = input.ReadSingle();
            pes.ParticleExitSpeed = input.ReadSingle();

            pes.LightRangeMax = input.ReadSingle();
            pes.LightRangeMin = input.ReadSingle();
            pes.LightFalloff = input.ReadSingle();
            pes.LightColorLow = input.ReadColor();
            pes.LightColorHigh = input.ReadColor();
            pes.LightFrames = input.ReadSingle();
            pes.LightScope = (ChiseDrive.Graphics.LightScope)input.ReadInt32();

            return pes;
        }
    }
}