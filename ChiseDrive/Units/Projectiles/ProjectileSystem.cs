using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ChiseDrive.Units
{
    public class ProjectileSystem
    {
        public ChiseDriveGame Game { get; set; }
        public ProjectileSystem(ChiseDriveGame game) { Game = game; }

        Cooldown cooldown = new Cooldown(Time.FromSeconds(1f));
        public Cooldown Cooldown
        {
            get
            {
                return cooldown;
            }
            set
            {
                cooldown = value;
            }
        }

        public virtual void Fire(IFollow emitobject, IFollow targetobject, float accuracy, ID shooterid)
        {

        }

        public virtual void Fire(IFollow emitobject, Vector3 targetpoint, float accuracy, ID shooterid)
        {
        }

        protected virtual void Fire(Vector3 emitpoint, Vector3 targetpoint, float accuracy, ID shooterid)
        {
            UnitEvent.Announce(EventType.ShotFired, shooterid, ID.None, 1f);
        }
    }
}