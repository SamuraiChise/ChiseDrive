using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace ChiseDrive.Units
{
    public class LaserSystem : ProjectileSystem
    {
        public static SoundBank SoundBank { get; set; }
        AudioEmitter audioEmitter;
        Cue cue;

        public LaserSystem(ChiseDriveGame game)
            : base(game)
        {
            audioEmitter = new AudioEmitter();
            audioEmitter.Velocity = Vector3.Zero;
            audioEmitter.Up = Vector3.Up;
            audioEmitter.Forward = Vector3.Forward;
            audioEmitter.DopplerScale = 30f;

            this.Cooldown = new Cooldown(Time.FromFrames(8f));
        }

        public override void Fire(IFollow emitobject, IFollow targetobject, float accuracy, ID shooterid)
        {
            audioEmitter.Position = emitobject.Position;

            if (cue == null || !cue.IsPrepared)
            {
                cue = SoundBank.GetCue("Guns");
                cue.Apply3D(Game.AudioListener, audioEmitter);
                cue.Play();
            }

            Laser.FireLaser(emitobject, emitobject.RotationPosition.Up, Laser.Caliber.n200mm, shooterid);
        }

        public override void Fire(IFollow emitobject, Vector3 targetpoint, float accuracy, ID shooterid)
        {
            audioEmitter.Position = emitobject.Position;

            if (cue == null || !cue.IsPrepared)
            {
                cue = SoundBank.GetCue("Guns");
                cue.Apply3D(Game.AudioListener, audioEmitter);
                cue.Play();
            }

            Laser.FireLaser(emitobject, emitobject.RotationPosition.Up, Laser.Caliber.n200mm, shooterid);
        }

        protected override void Fire(Vector3 emitpoint, Vector3 targetpoint, float accuracy, ID shooterid)
        {
        }
    }
}