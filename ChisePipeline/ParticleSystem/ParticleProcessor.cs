#region File Description
//-----------------------------------------------------------------------------
// ParticleProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
#endregion

namespace ChisePipeline
{
    /// <summary>
    /// Custom Content Pipeline processor class for particle system settings.
    /// </summary>
    [ContentProcessor]
    class ParticleProcessor : ContentProcessor<ParticleSettingsContent,
                                               ParticleSettingsContent>
    {
        public override ParticleSettingsContent Process(ParticleSettingsContent input,
                                                        ContentProcessorContext context)
        {
            // Create an effect material describing how we want to draw these particles.
            EffectMaterialContent effect = new EffectMaterialContent();

            // Set the effect to be used by this material.
            effect.Effect = new ExternalReference<EffectContent>(
                                                        "Effects/ParticleEffect.fx");

            // Set the texture to be used by this material.
            effect.Textures["Texture"] = new ExternalReference<TextureContent>(
                                                        input.TextureName + ".png");

            // Set parameters describing how the material should be rendered.
            effect.OpaqueData.Add("Duration", (float)input.Duration.TotalSeconds);
            effect.OpaqueData.Add("DurationRandomness", input.DurationRandomness);
            effect.OpaqueData.Add("Gravity", input.Gravity);
            effect.OpaqueData.Add("EndVelocity", input.EndVelocity);
            effect.OpaqueData.Add("MinColor", input.MinColor.ToVector4());
            effect.OpaqueData.Add("MaxColor", input.MaxColor.ToVector4());

            effect.OpaqueData.Add("RotateSpeed",
                new Vector2(input.MinRotateSpeed, input.MaxRotateSpeed));

            effect.OpaqueData.Add("StartSize",
                new Vector2(input.MinStartSize, input.MaxStartSize));

            effect.OpaqueData.Add("EndSize",
                new Vector2(input.MinEndSize, input.MaxEndSize));

            // Choose the appropriate effect technique. If these particles will never
            // rotate, we can use a simpler pixel shader that requires less GPU power.
            if ((input.MinRotateSpeed == 0) && (input.MaxRotateSpeed == 0))
                input.TechniqueName = "NonRotatingParticles";
            else
                input.TechniqueName = "RotatingParticles";

            // Chain to the built-in MaterialProcessor, telling it to convert the
            // EffectMaterialContent that we just created. This will automatically
            // compile the .fx file and texture that the material is referencing,
            // and fill in the returned material with references to where the compiled
            // effect and texture have been stored. Because of this, there is no longer
            // any need to explicitly add the particle effect or textures to the
            // project: they will now automatically be built and loaded any time they
            // are referenced by one of our particle description XML files.
            input.ParticleEffect = context.Convert<MaterialContent,
                                                   MaterialContent>(effect,
                                                                "MaterialProcessor");

            return input;
        }
    }
}
