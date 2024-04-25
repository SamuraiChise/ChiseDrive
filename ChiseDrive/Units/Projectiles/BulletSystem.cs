using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace ChiseDrive.Units
{
    public class BulletSystem : ProjectileSystem
    {
        public static SoundBank SoundBank { get; set; }
        AudioEmitter audioEmitter;
        Cue cue;
        public Bullet.Caliber caliber;

        public BulletSystem(ChiseDriveGame game)
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
            Vector3 totarget = targetobject.Position - emitobject.Position;
            float distance = Math.Abs(totarget.Length());
            float timetotarget = distance / Bullet.Speed(Bullet.Caliber.c50mm);
            Vector3 projectedlocation = targetobject.Position 
                + (targetobject.Velocity * timetotarget);

            Fire(emitobject.Position, projectedlocation, accuracy, shooterid);
        }

        public override void Fire(IFollow emitobject, Vector3 targetpoint, float accuracy, ID shooterid)
        {
            Fire(emitobject.Position, targetpoint, accuracy, shooterid);
        }

        protected override void Fire(Vector3 emitpoint, Vector3 targetpoint, float accuracy, ID shooterid)
        {
            audioEmitter.Position = emitpoint;

            if (cue == null || !cue.IsPrepared)
            {
                cue = SoundBank.GetCue("Guns");
                cue.Apply3D(Game.AudioListener, audioEmitter);
                cue.Play();
            }

            float inaccuracy = 1f - accuracy;
            inaccuracy *= 2f;

            Vector3 inaccuratePoint = Helper.RandomDisplacement(targetpoint, inaccuracy);
            Vector3 direction = inaccuratePoint - emitpoint;

            Helper.Normalize(ref direction);
            Bullet.FireBullet(emitpoint, direction, caliber, shooterid);
            base.Fire(emitpoint, targetpoint, accuracy, shooterid);
        }
    }
}