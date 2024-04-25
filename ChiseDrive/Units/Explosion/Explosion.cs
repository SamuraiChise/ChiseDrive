using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ChiseDrive.Particles;
using ChiseDrive.Graphics;

namespace ChiseDrive.Units
{
    public class Explosion : IFollow, ILightEmitter, IDisposable
    {
        const int MaxExplosions = 100;
        const int MaxDebris = 400;

        static ParticleEmitterSettings fireSettings;
        static ParticleEmitterSettings smokeSettings;
        static ParticleEmitterSettings debrisSettings;
        static Explosion[] explosions;
        static DebrisStreamer[] debris;

        static int activeExplosions;
        static int activeDebris;

        static ChiseDriveGame Game;

        static SoundBank soundBank;
        static Cooldown SoundCooldown = new Cooldown(Time.FromFrames(10f));

        public static void Initialize(ChiseDriveGame game, SoundBank soundbank, ParticleEmitterSettings fire, ParticleEmitterSettings smoke, ParticleEmitterSettings debris)
        {
            soundBank = soundbank;

            Game = game;
            
            Explosion.fireSettings = fire;
            Explosion.smokeSettings = smoke;
            Explosion.debrisSettings = debris;

            ParticleSystem.TryGetCreateSystem(fire.ParticleSystemName);
            ParticleSystem.TryGetCreateSystem(smoke.ParticleSystemName);
            ParticleSystem.TryGetCreateSystem(debris.ParticleSystemName);

            Explosion.explosions = new Explosion[MaxExplosions];
            for (int i = 0; i < MaxExplosions; i++)
            {
                Explosion.explosions[i] = new Explosion(fireSettings, smokeSettings);
            }

            Explosion.debris = new DebrisStreamer[MaxDebris];
            for (int i = 0; i < MaxDebris; i++)
            {
                Explosion.debris[i] = new DebrisStreamer(debrisSettings);
            }

            activeDebris = 0;
            activeExplosions = 0;
        }

        public static void UpdateAll(Time elapsed)
        {
            SoundCooldown.Update(elapsed);

            for (int i = 0; i < activeExplosions; i++)
            {
                explosions[i].Update(elapsed);
            }
            for (int i = 0; i < activeDebris; i++)
            {
                debris[i].Update(elapsed);
            }

            ChiseDrive.Debug.Metrics.OpenMetric("DEBRIS");
            CompactDebris();
            ChiseDrive.Debug.Metrics.CloseMetric("DEBRIS");

            ChiseDrive.Debug.Metrics.OpenMetric("COMPACT");
            CompactExplosions();
            ChiseDrive.Debug.Metrics.CloseMetric("COMPACT");

            //ChiseDrive.Debug.DebugText.Write("Active Explosions [" + activeExplosions + "] Debris [" + activeDebris + "]");
        }

        public static void DisposeAll()
        {
            for (int i = 0; i < explosions.Length; i++)
            {
                explosions[i].Dispose();
                explosions[i] = null;
            }

            for (int i = 0; i < debris.Length; i++)
            {
                debris[i].Dispose();
                debris[i] = null;
            }

            explosions = null;
            debris = null;

            fireSettings = null;
            smokeSettings = null;
            debrisSettings = null;
        }

        public static void ResetAll()
        {
            for (int i = 0; i < explosions.Length; i++)
            {
                explosions[i].Reset();
            }
            for (int i = 0; i < debris.Length; i++)
            {
                debris[i].Reset();
            }
        }

        public static void CompactExplosions()
        {
            int compactplace = 0;
            if (activeExplosions == 0) return;

            for (int i = 0; i < explosions.Length; i++)
            {
                if (explosions[i].IsDone)
                {
                    explosions[i].TryRemoveLights();

                    // We have an expired explosion!
                    // Assume it's our last one!
                    activeExplosions = i;

                    // Keep track of the last place we found good data
                    if (compactplace < i) compactplace = i;
                    for (int k = compactplace; k <= explosions.Length; k++)
                    {
                        if (k == explosions.Length) return;
                        if (!explosions[k].IsDone)
                        {
                            // We found an active one!
                            Explosion temp = explosions[i];
                            explosions[i] = explosions[k];
                            explosions[k] = temp;
                            compactplace = k + 1;
                            break;// for k <= explosions.Length
                        }
                    }
                }
            }

        }
        public static void CompactDebris()
        {
            int compactplace = 0;
            if (activeDebris == 0) return;

            for (int i = 0; i < debris.Length; i++)
            {
                if (debris[i].IsDone)
                {
                    // We have an expired explosion!
                    // Assume it's our last one!
                    activeDebris = i;

                    // Keep track of the last place we found good data
                    if (compactplace < i) compactplace = i;
                    for (int k = compactplace; k <= debris.Length; k++)
                    {
                        if (k == debris.Length) return;
                        if (!debris[k].IsDone)
                        {
                            // We found an active one!
                            DebrisStreamer temp = debris[i];
                            debris[i] = debris[k];
                            debris[k] = temp;
                            compactplace = k + 1;
                            break;// for k <= debris.Length
                        }
                    }
                }
            }
        }

        ParticleEmitter fire;
        ParticleEmitter smoke;

        public List<PointLight> Lights
        {
            get
            {
                return fire.Lights;
            }
        }

        IFollow parent;
        ILitObject litobject;
        Vector3 position;
        Vector3 velocity;

        AudioEmitter audioEmitter;
        Cue boomcue;
        float scale;
        bool smokewaiting = true;

        #region IFollow
        public Vector3 Position
        {
            get
            {
                return parent != null ? parent.Position + position : position;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public Vector3 Velocity
        {
            get
            {
                return parent != null ? parent.Velocity : velocity;
            }
        }
        public Matrix RotationPosition
        {
            get
            {
                return parent != null ? parent.RotationPosition : Matrix.Identity;
            }
        }
        public Quaternion Rotation
        {
            get
            {
                return parent != null ? parent.Rotation : Quaternion.Identity;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public float Scale
        {
            get
            {
                return scale;
            }
        }
        #endregion

        bool active = false;

        bool IsDone
        {
            get
            {
                return !active;
            }
        }

        void Reset()
        {
            active = false;
            fire.Reset();
            smoke.Reset();
        }

        Explosion(ParticleEmitterSettings firesettings, ParticleEmitterSettings smokesettings)
        {
            this.fire = new ParticleEmitter(this, firesettings);
            this.smoke = new ParticleEmitter(this, smokesettings);
        }

        public void EmitInitialize(IFollow parent, Vector3 position, float scale, Vector3 velocity)
        {
            this.parent = parent;
            this.position = position;
            this.scale = scale;
            this.velocity = velocity;

            if (SoundCooldown.IsReady)
            {
                if (audioEmitter == null) audioEmitter = new AudioEmitter();
                audioEmitter.Position = position;
                audioEmitter.Velocity = velocity;
                audioEmitter.Up = Vector3.Up;
                audioEmitter.Forward = Vector3.Forward;
                audioEmitter.DopplerScale = 30f;

                if (boomcue == null || !boomcue.IsPrepared)
                {
                    boomcue = soundBank.GetCue("Explosion");
                    boomcue.Apply3D(Game.AudioListener, audioEmitter);
                    boomcue.Play();
                    SoundCooldown.Trigger();
                }
            }
            
            fire.EmitOnce();
            smokewaiting = true;

            LightObject(parent as ILitObject);
            active = true;
        }

        public static void Emit(Vector4 origin_scale, Vector3 velocity, Vector3 debrisDirection)
        {
            if (Explosion.explosions == null) throw new Exception("Call Explosion.Initialize!");

            explosions[activeExplosions].EmitInitialize(
                null, new Vector3(origin_scale.X, origin_scale.Y, origin_scale.Z),
                origin_scale.W, velocity);

            GenerateDebris(debrisDirection);
            activeExplosions++;
        }

        public static void Emit(Vector4 origin_scale)
        {
            if (Explosion.explosions == null) throw new Exception("Call Explosion.Initialize!");

            explosions[activeExplosions].EmitInitialize(
                null, new Vector3(origin_scale.X, origin_scale.Y, origin_scale.Z),
                origin_scale.W, Vector3.Zero);

            GenerateDebris(Vector3.Zero);
            activeExplosions++;
        }

        public static void Emit(IFollow origin, float scale)
        {
            if (Explosion.explosions == null) throw new Exception("Call Explosion.Initialize!");

            explosions[activeExplosions].EmitInitialize(
                origin, Vector3.Zero, scale, origin.Velocity);

            GenerateDebris(Vector3.Zero);
            activeExplosions++;
         }

        public static void Emit(IFollow origin, Vector3 offset, Vector3 debrisDirection, float scale)
        {
            if (Explosion.explosions == null) throw new Exception("Call Explosion.Initialize!");

            if (activeExplosions >= MaxExplosions) return;

            explosions[activeExplosions].EmitInitialize(
                origin, offset, scale, origin.Velocity);

            GenerateDebris(debrisDirection);
            activeExplosions++;
        }

        public static void GenerateDebris(Vector3 direction)
        {
            if (activeDebris >= MaxDebris) return;

            float scale = explosions[activeExplosions].Scale;
            int numdebris = (int)(Helper.Randomf * scale * 1.5f);

            for (int i = 0; i < numdebris && activeDebris < MaxDebris; i++)
            {
                Vector3 randomVelocity = Helper.RandomDisplacement(Vector3.Zero, scale * 0.1f, scale * 0.3f);
                debris[activeDebris].Emit(
                    explosions[activeExplosions].Position,
                    direction + randomVelocity);

                activeDebris++;
            }
        }

        public void Dispose()
        {
            fire.Dispose();
            smoke.Dispose();

            fire = null;
            smoke = null;
        }

        void LightObject(ILitObject litobject)
        {/*
            this.litobject = litobject;

            if (this.litobject != null)
            {
                foreach (PointLight pl in Lights)
                {
                    this.litobject.AddLight(pl);
                }
            }*/
        }

        void UnLightObject()
        {/*
            if (this.litobject != null)
            {
                foreach (PointLight pl in Lights)
                {
                    this.litobject.RemoveLight(pl);
                }
            }
            this.litobject = null;*/
        }

        void TryRemoveLights()
        {
            if (this.litobject != null)
            {
                UnLightObject();
            }
        }

        bool CheckDone()
        {
            if (!active) return false;
            if (!fire.IsDone) return false;
            if (!smoke.IsDone) return false;
            if (boomcue != null && !boomcue.IsStopped) return false;
            //foreach (DebrisStreamer d in debris)
            //{
            //    if (!d.IsDone) return false;
            //}
            // Everything seems to be done, so we're no longer active!
            active = false;
            return true;
        }

        void Update(Time elapsed)
        {
            fire.Update(elapsed);

            if (smokewaiting && fire.IsDone)
            {
                smoke.EmitOnce();
                smokewaiting = false;
            }

            smoke.Update(elapsed);

            CheckDone();
        }
    }
}