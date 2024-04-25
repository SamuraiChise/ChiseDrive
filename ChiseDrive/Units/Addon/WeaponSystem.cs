using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChiseDrive.Motion;
using ChiseDrive.Particles;
using ChiseDrive.Graphics;

namespace ChiseDrive.Units
{
    public class WeaponSystem : System, ILightEmitter
    {
        public override string Name { get; set; }
        public List<PointLight> Lights
        {
            get
            {
                List<PointLight> lights = new List<PointLight>();
                foreach (Weapon w in weapons)
                {
                    lights.AddRange(w.Lights);
                }
                return lights;
            }
        }

        static public WeaponSystem TryBuildSystem(
            List<AttachmentPoint> attachments,
            ParticleEmitterSettings emitsettings, 
            Cooldown weaponcooldown,
            ProjectileSystem projectileSystem)
        {
            WeaponSystem system = new WeaponSystem();

            foreach (AttachmentPoint ap in attachments)
            {
                system.weapons.Add(new Weapon(ap, emitsettings, weaponcooldown, projectileSystem));
            }

            if (system.weapons.Count > 0)
            {
                system.cyclecooldown = new Cooldown(system.weapons[0].Cooldown.ResetTime / system.weapons.Count);
                return system;
            }
            else return null;
        }

        List<Weapon> weapons = new List<Weapon>();
        int currentweapon = 0;
        Cooldown cyclecooldown;

        /// <summary>
        /// If there is a ready weapon, it will return it.
        /// Otherwise it will return null.
        /// </summary>
        public Weapon NextReadyWeapon
        {
            get
            {
                Weapon retval = null;
                if (weapons[currentweapon].Cooldown.IsReady)
                {
                    retval = weapons[currentweapon];
                    currentweapon++;
                    if (currentweapon >= weapons.Count) currentweapon = 0;
                }
                return retval;
            }
        }

        /// <summary>
        /// If there is a ready weapon on a spaced cycle
        /// it will return it, else null.
        /// </summary>
        public Weapon NextTimedWeapon
        {
            get
            {
                Weapon retval = null;
                if (cyclecooldown.IsReady)
                {
                    retval = NextReadyWeapon;
                    cyclecooldown.Trigger();
                }
                return retval;
            }
        }

        public float FireArc
        {
            get
            {
                if (weapons.Count > 0)
                {
                    return weapons[0].FireArc;
                }
                return 1f;
            }
            set
            {
                foreach (Weapon w in weapons)
                {
                    w.FireArc = value;
                }
            }
        }

        public int AmmoLoaded
        {
            get
            {
                int ammo = 0;
                foreach (Weapon w in weapons)
                {
                    ammo += w.AmmoAvailable;
                }
                return ammo;
            }
        }

        public int AmmoMax
        {
            set
            {
                int perweapon = value / weapons.Count;
                if (perweapon < 1) throw new Exception("Trying to load weapons with 0 ammo.  Total weapons: " + weapons.Count + " Ammo to distribute: " + value);
                foreach (Weapon w in weapons)
                {
                    w.AmmoMax = perweapon;
                }
            }
            get
            {
                int ammo = 0;
                foreach (Weapon w in weapons)
                {
                    ammo += w.AmmoMax;
                }
                return ammo;
            }
        }

        public float AmmoPercent
        {
            get
            {
                float percent = 0f;
                foreach (Weapon w in weapons)
                {
                    percent += w.AmmoPercent;
                }
                percent /= weapons.Count;
                return percent;
            }
        }

        public void Reload()
        {
            foreach (Weapon w in weapons)
            {
                w.AmmoAvailable = w.AmmoMax;
            }
        }

        public void Reload(int ammount)
        {
            foreach (Weapon w in weapons)
            {
                w.AmmoAvailable += ammount;
            }
        }

        public override void Dispose()
        {
            foreach (Weapon w in weapons)
            {
                w.Dispose();
            }
            weapons.Clear();
        }

        public override void Update(Time elapsed)
        {
            cyclecooldown.Update(elapsed);

            foreach (Weapon w in weapons)
            {
                w.Update(elapsed);
            }
        }
    }
}