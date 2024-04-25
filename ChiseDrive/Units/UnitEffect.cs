using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ChiseDrive.Particles;

namespace ChiseDrive.Units
{
    class UnitEffect
    {
        ParticleEmitter emitter;

        public void TriggerAt(Vector3 position)
        {/*
            if (emitter != null) // We have an effect to process
            {
                if (emitter.CuePlayFrequency == CuePlayFrequency.Always
                    || VoiceCooldown.IsReady)// We can try to play something
                {
                    if (effect.CueName != null && SoundBank != null)// And we have a cue to try
                    {
                        if (Cue == null || !Cue.IsPrepared)// Nothing else is playing
                        {
                            Cue = SoundBank.GetCue(effect.CueName);
                            Cue.Apply3D(Cameras.Camera.AudioListener, audioEmitter);
                            Cue.Play();
                            if (effect.CuePlayFrequency == CuePlayFrequency.Voice) VoiceCooldown.Trigger();
                        }
                    }
                }

                if (effect.Emitter != null)
                {
                    effect.Emitter.EmitOnce(location);
                }
            }*/
        }

        public bool IsDone { get; set; }
    }
}
