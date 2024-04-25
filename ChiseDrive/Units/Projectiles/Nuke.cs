using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Physics;
using ChiseDrive.Particles;
using ChiseDrive.Motion;
using ChiseDrive.Graphics;

namespace ChiseDrive.Units
{
    class Nuke : Unit
    {
        Timer liferemaining = new Timer(Time.FromSeconds(1.75f));
        ParticleEmitter fireEmitter;
        ParticleEmitter smokeEmitter;

        ID shooterid;

        public override BoundingSphere BoundingSphere
        {
            get
            {
                BoundingSphere sphere = base.BoundingSphere;
                sphere.Radius = 10f;
                return sphere;
            }
        }

        static public void BlindMissile(
            ChiseDriveGame game,
            IFollow origin,
            Vector3 destination,
            ParticleEmitterSettings fire,
            ParticleEmitterSettings smoke,
            LitObject model,
            ID shooterid)
        {
            Nuke newmissile = new Nuke(game, fire, smoke, model, shooterid);
            newmissile.Locomotion.MoveTo(destination);
            newmissile.LaunchPhysics(origin);
        }

        static public void LockMissile(
            ChiseDriveGame game,
            IFollow origin,
            IFollow target,
            ParticleEmitterSettings fire,
            ParticleEmitterSettings smoke,
            LitObject model,
            ID shooterid)
        {
            Nuke newmissile = new Nuke(game, fire, smoke, model, shooterid);
            newmissile.Target = target;
            newmissile.Locomotion.MoveTo(newmissile.Target.Position);
            newmissile.LaunchPhysics(origin);
        }

        void LaunchPhysics(IFollow origin)
        {
            Position = origin.Position;

            // Conservation of momentum!
            Physics.Push(origin.Velocity);

            Locomotion.Update(Time.Zero);
        }

        Nuke(ChiseDriveGame game, ParticleEmitterSettings fire, ParticleEmitterSettings smoke, LitObject body, ID shooterid)
            : base(game)
        {
            this.fireEmitter = new ParticleEmitter(this, fire);
            this.smokeEmitter = new ParticleEmitter(this, smoke);
            this.fireEmitter.EmitLoop();
            this.smokeEmitter.EmitLoop();

            this.shooterid = shooterid;

            this.Active = true;
            this.MaxHealth = 1f;

            this.Physics = new BasicPhysics();

            this.Locomotion = new MissileMotion(this);
            this.Locomotion.Throttle = 1f;
            this.Locomotion.AccelerationRate = 2f;
            this.Locomotion.MaxSpeed = 4f;
            this.Locomotion.TurnRate = (float)Math.PI / 120f;

            foreach (Unit u in Game.Units)
            {
                if (u.ID == shooterid)
                {
                    Faction = u.Faction;
                }
            }

            this.Scale = 8f;
            this.DeathByExplosion = true;

            this.body = body;
            this.Visible = true;

            UnitEvent.Announce(EventType.ShotFired, shooterid, ID.None, 1f);
        }

        public override void Dispose()
        {
            fireEmitter.Dispose();
            fireEmitter = null;

            smokeEmitter.Dispose();
            smokeEmitter = null;

            liferemaining.Set(Time.Zero);
            base.Dispose();
        }
        protected override void EndDeath()
        {
            base.EndDeath();
            Dispose();
        }
        bool donehit = false;
        protected override void UpdateDeath(Time elapsed)
        {
            const float KillRadius = 90f;
            const float KillDamage = 18f;

            bool dodamage = false;

            if (!triggeredDamage1 && deathtime.Frames < quarterDeath * 3f)
            {
                dodamage = true;
                triggeredDamage1 = true;
            }

            if (!triggeredDamage2 && deathtime.Frames < quarterDeath * 2f)
            {
                dodamage = true;
                triggeredDamage2 = true;
            }

            if (!triggeredDamage3 && deathtime.Frames < quarterDeath)
            {
                dodamage = true;
                triggeredDamage3 = true;
            }

            if (dodamage)
            {
                foreach (Unit u in Game.Units)
                {
                    float distance = Vector3.Distance(u.Position, Position);
                    if (distance < KillRadius)
                    {
                        if (u.ID != shooterid && !(u is Nuke)
                            && Faction.GetAlignment(u.Faction) != Alignment.Friendly)
                        {
                            u.TakeDamage(KillDamage, shooterid);
                            if (!donehit) UnitEvent.Announce(EventType.WeaponHit, shooterid, ID.None, 1f);
                            donehit = true;
                        }
                    }
                }
            }
            base.UpdateDeath(elapsed);
        }
        bool triggeredDamage1 = false;
        bool triggeredDamage2 = false;
        bool triggeredDamage3 = false;
        float quarterDeath = 0f;
        void Detonate()
        {
            Locomotion.Throttle = 0f;
            StartDeath();

            quarterDeath = deathtime.Frames * 0.25f;

            Body.Visible = false;
        }

        public override float CollisionDamage
        {
            get
            {
                return 20f;
            }
        }

        Cooldown ProximityCheck = new Cooldown(Time.FromFrames(5f));

        public override void Update(Time elapsed)
        {
            base.Update(elapsed);

            if (IsAlive)
            {
                if (Target != null) Locomotion.MoveTo(Target.Position);

                // Check for proximity detonation
                if (ProximityCheck.AutoTrigger(elapsed))
                {
                    const float DetonationRange = 30f;
                    foreach (Unit u in Game.Units)
                    {
                        float distance = Vector3.Distance(u.Position, Position);
                        if (u.IsAlive && u.ID != shooterid && !(u is Nuke)
                            && Faction.GetAlignment(u.Faction) != Alignment.Friendly
                            && (distance - u.BoundingSphere.Radius * 0.5f) < DetonationRange) 
                        {
                            Detonate();
                        }
                    }
                }

                fireEmitter.Update(elapsed);
                smokeEmitter.Update(elapsed);

                liferemaining.SubTime(elapsed);
                if (liferemaining.IsZero) Detonate();
            }
        }
    }
}