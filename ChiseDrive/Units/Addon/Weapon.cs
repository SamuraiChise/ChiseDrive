using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Motion;
using ChiseDrive.Graphics;
using ChiseDrive.Particles;

namespace ChiseDrive.Units
{
    public class Weapon : SimpleEmitter
    {
        public enum Result
        {
            WeaponFired,
            WeaponCooldown,
            OutOfAmmo,
            AllyTargeted,
        };

        ProjectileSystem projectile;
        Cooldown cooldown;

        int ammoMax;
        int ammoLoaded;

        public Cooldown Cooldown
        {
            get
            {
                return cooldown;
            }
        }
        public int AmmoAvailable
        {
            get
            {
                return ammoLoaded;
            }
            set
            {
                ammoLoaded = value;
                Helper.Clamp(ref ammoLoaded, 0, ammoMax);
            }
        }
        public int AmmoMax
        {
            get
            {
                return ammoMax;
            }
            set
            {
                ammoMax = value;
                Helper.Clamp(ref ammoMax, 0, int.MaxValue);
            }
        }
        public float AmmoPercent
        {
            get
            {
                return (float)ammoLoaded / (float)ammoMax;
            }
        }
        float firearc = 1f;
        public float FireArc
        {
            get
            {
                return firearc;
            }
            set
            {
                if (value == 0f || value > (float)Math.PI) throw new Exception("Invalid Fire Arc (" + value + ") use a value between 0 and PI");
                firearc = value;
            }
        }
        public float Range
        {
            get
            {
                return 1000f;
            }
            set
            {
            }
        }

        public Weapon(AttachmentPoint attachment, ParticleEmitterSettings emitsettings, Cooldown weaponcooldown, ProjectileSystem projectileSystem) : base(attachment, emitsettings)
        {
            projectile = projectileSystem;
            cooldown = weaponcooldown;
        }

        public bool InFireArc(Vector3 targetpoint)
        {
            Matrix weaponview = Matrix.CreateLookAt(
                Position, Position + Attachment.Up,
                Vector3.Up);
            Matrix weaponproj = Matrix.CreatePerspectiveFieldOfView(FireArc, 1f, 1f, Range);

            BoundingFrustum frustum = new BoundingFrustum(weaponview * weaponproj);
            ContainmentType containment = frustum.Contains(targetpoint);

            if (containment == ContainmentType.Disjoint) return false;
            return true;
        }

        public Result FireAt(IFollow target, float accuracy, ID shooterid)
        {
            if (ammoLoaded == 0) return Result.OutOfAmmo;

            if (cooldown.IsReady)
            {
                cooldown.Trigger();
                if (emitter != null) emitter.EmitOnce();
                projectile.Fire(this, target, accuracy, shooterid);
                ammoLoaded--;
                return Result.WeaponFired;
            }
            else
            {
                return Result.WeaponCooldown;
            }
        }

        public Result FireAt(Vector3 point, float accuracy, ID shooterid)
        {
            if (ammoLoaded == 0) return Result.OutOfAmmo;

            if (cooldown.IsReady)
            {
                cooldown.Trigger();
                if (emitter != null) emitter.EmitOnce();
                projectile.Fire(this, point, accuracy, shooterid);
                ammoLoaded--;
                return Result.WeaponFired;
            }
            else
            {
                return Result.WeaponCooldown;
            }
        }

        public Result FireBlind(float accuracy, ID shooterid)
        {
            Vector3 target = Position + Attachment.Up * 1000f;
            return FireAt(target, accuracy, shooterid);
        }

        public void TriggerCooldown()
        {
            cooldown.Trigger();
        }

        public override void Update(Time elapsed)
        {
            cooldown.Update(elapsed);
            base.Update(elapsed);
        }
    }
}
