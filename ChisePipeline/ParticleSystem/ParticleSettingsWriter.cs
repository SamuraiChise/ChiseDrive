#region File Description
//-----------------------------------------------------------------------------
// ParticleSettingsWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace ChisePipeline
{
    /// <summary>
    /// Content Pipeline class for saving ParticleSettings data into XNB format.
    /// </summary>
    [ContentTypeWriter]
    class ParticleSettingsWriter : ContentTypeWriter<ParticleSettingsContent>
    {
        protected override void Write(ContentWriter output,
                                      ParticleSettingsContent value)
        {
            output.WriteObject(value.ParticleEffect);
            output.Write(value.TechniqueName);
            output.Write(value.MaxParticles);
            output.WriteObject(value.Duration);
            output.Write(value.EmitterVelocitySensitivity);
            output.Write(value.MinHorizontalVelocity);
            output.Write(value.MaxHorizontalVelocity);
            output.Write(value.MinVerticalVelocity);
            output.Write(value.MaxVerticalVelocity);
            output.WriteObject(value.SourceBlend);
            output.WriteObject(value.DestinationBlend);
        }


        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "ChiseDrive.Particles.ParticleSettings, ChiseDrive";
        }


        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "ChiseDrive.Particles.ParticleSettingsReader, ChiseDrive";
        }
    }
}
