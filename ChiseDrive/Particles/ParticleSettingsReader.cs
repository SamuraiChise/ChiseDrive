#region File Description
//-----------------------------------------------------------------------------
// ParticleSettingsReader.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace ChiseDrive.Particles
{
    /// <summary>
    /// Content Pipeline class for loading ParticleSettings data from XNB format.
    /// </summary>
    class ParticleSettingsReader : ContentTypeReader<ParticleSettings>
    {
        protected override ParticleSettings Read(ContentReader input,
                                                 ParticleSettings existingInstance)
        {
            ParticleSettings settings = new ParticleSettings();

            settings.ParticleEffect = input.ReadObject<Effect>();
            settings.TechniqueName = input.ReadString();
            settings.MaxParticles = input.ReadInt32();
            settings.Duration = input.ReadObject<TimeSpan>();
            settings.EmitterVelocitySensitivity = input.ReadSingle();
            settings.MinHorizontalVelocity = input.ReadSingle();
            settings.MaxHorizontalVelocity = input.ReadSingle();
            settings.MinVerticalVelocity = input.ReadSingle();
            settings.MaxVerticalVelocity = input.ReadSingle();
            settings.SourceBlend = input.ReadObject<Blend>();
            settings.DestinationBlend = input.ReadObject<Blend>();

            return settings;
        }
    }
}
