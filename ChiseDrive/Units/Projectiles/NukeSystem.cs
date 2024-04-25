using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using ChiseDrive.Particles;
using ChiseDrive.Motion;
using ChiseDrive.Graphics;

namespace ChiseDrive.Units
{
    public class NukeSystem : ProjectileSystem
    {
        public static SoundBank SoundBank { get; set; }
        const float MissileScale = 1.5f;

        ParticleEmitterSettings fire;
        ParticleEmitterSettings smoke;
        Model missilebody;
        Material missilematerial;
        Effect missileeffect;
        
        Cue cue;
        AudioEmitter audioEmitter;

        public NukeSystem(ChiseDriveGame game, Model missilebody, Material missilematerial, Effect missileeffect, ParticleEmitterSettings fire, ParticleEmitterSettings smoke)
            : base(game)
        {
            this.fire = fire;
            this.smoke = smoke;
            this.missilebody = missilebody;
            this.missilematerial = missilematerial;
            this.missileeffect = missileeffect;
            this.Cooldown = new Cooldown(Time.FromSeconds(2f));

            audioEmitter = new AudioEmitter();
            audioEmitter.Velocity = Vector3.Zero;
            audioEmitter.Up = Vector3.Up;
            audioEmitter.Forward = Vector3.Forward;
            audioEmitter.DopplerScale = 30f;

            ParticleSystem.TryGetCreateSystem(smoke.ParticleSystemName);
            ParticleSystem.TryGetCreateSystem(fire.ParticleSystemName);
        }
        
        public override void Fire(IFollow emitobject, Vector3 targetpoint, float accuracy, ID shooterid)
        {
            audioEmitter.Position = emitobject.Position;
            Vector3 velocity = targetpoint - emitobject.Position;
            velocity.Normalize();
            velocity *= 4f;
            audioEmitter.Velocity = velocity;
            if (cue == null || !cue.IsPrepared)
            {
                cue = SoundBank.GetCue("Missile");
                cue.Apply3D(Game.AudioListener, audioEmitter);
                cue.Play();
            }
            GameModel model = new GameModel(missilebody, MissileScale);
            LitObject missile = new LitObject(Game, model, missilematerial, missileeffect);
            Nuke.BlindMissile(Game, emitobject, targetpoint, fire, smoke, missile, shooterid);
        }

        public override void Fire(IFollow emitobject, IFollow targetobject, float accuracy, ID shooterid)
        {
            audioEmitter.Position = emitobject.Position;
            Vector3 velocity = targetobject.Position - emitobject.Position;
            velocity.Normalize();
            velocity *= 4f;
            audioEmitter.Velocity = velocity;
            if (cue == null || !cue.IsPrepared)
            {
                cue = SoundBank.GetCue("Missile");
                cue.Apply3D(Game.AudioListener, audioEmitter);
                cue.Play();
            }
            GameModel model = new GameModel(missilebody, MissileScale);
            LitObject missile = new LitObject(Game, model, missilematerial, missileeffect);
            Nuke.LockMissile(Game, emitobject, targetobject, fire, smoke, missile, shooterid);
        }
    }
}