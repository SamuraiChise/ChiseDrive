using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ChiseDrive.Particles;

// TODO: replace this with the type you want to write out.
using TWrite = ChiseDrive.Particles.ParticleEmitterSettings;

namespace ChisePipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class ParticleEmitterWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write(ContentWriter output, TWrite value)
        {
            output.Write(value.ParticleSystemName);
            output.Write(value.SoundCueName);

            output.Write(value.ParticlesPerSecond);
            output.Write(value.ParticlesPerEmit);
            output.Write(value.ParticleExitSpeed);

            output.Write(value.LightRangeMax);
            output.Write(value.LightRangeMin);
            output.Write(value.LightFalloff);
            output.Write(value.LightColorLow);
            output.Write(value.LightColorHigh);
            output.Write(value.LightFrames);
            output.Write((int)value.LightScope);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // TODO: change this to the name of your ContentTypeReader
            // class which will be used to load this data.
            return "ChiseDrive.Particles.ParticleEmitterSettingsReader, ChiseDrive";
        }
    }
}
