using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ChiseDrive.Graphics;

namespace ChiseDrive.Units
{
    class Body : IDisposable, IFollow, IDestructible, IBounding, ILitObject, ILightEmitter
    {
        List<Body> components = new List<Body>();
        ID id;

        bool isdead = false;
        float health;
        float maxhealth;

        public virtual bool IsDead
        {
            get
            {
                return isdead;
            }
            set
            {
                isdead = value;
            }
        }
        public virtual bool IsDying
        {
            get
            {
                return !deathtime.IsZero ? true : false;
            }
        }
        public virtual bool IsAlive
        {
            get
            {
                return !IsDying && !IsDead;
            }
        }

        public virtual Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }
        public virtual Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }
        public Matrix PositionRotation
        {
            get
            {
                return Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);
            }
        }
        public float Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }

        public float Health
        {
            get
            {
                return health / maxhealth;
            }
        }
        public float MaxHealth
        {
            set
            {
                health = maxhealth = value;
            }
        }
        public virtual float CollisionDamage
        {
            get
            {
                return scale;
            }
        }

        Timer deathtime = Time.Zero;
        Cooldown newexplosion = new Cooldown(Time.FromSeconds(2f));
        List<PointLight> ExplosionLights = new List<PointLight>();

        public bool DeathByExplosion { get; set; }

        public void TakeHealing(float value, ID source)
        {
            if (IsDead) isdead = false;
            if (IsDying) deathtime = Time.Zero;

            health += value;

            if (health > maxhealth) health = maxhealth;
        }
        public void TakeDamage(float value, ID source)
        {
            if (!IsAlive) return;
            if (source == ID) return;

            health -= value;
            if (health <= 0f) StartDeath();
        }
        public void StartDeath()
        {
            deathtime = Time.FromSeconds(scale / 6f);
        }
        void UpdateDeath(Time elapsed)
        {
            deathtime -= elapsed;
            if (DeathByExplosion) BigExplosion();
            if (deathtime.IsZero) EndDeath();
        }
        void BigExplosion()
        {
            for (int i = 0; i < 1 + ((int)scale % 4); i++)
            {
                float scalear = 5f * scale;
                Vector4 originscale = new Vector4(Position, Helper.Randomf * 2f + 2f);
                Vector3 offset = Vector3.One * scalear;
                offset.X *= Helper.Randomf;
                offset.Y *= Helper.Randomf;
                offset.Z *= Helper.Randomf;
                offset.X *= Helper.Random % 2 == 0 ? -1f : 1f;
                offset.Y *= Helper.Random % 2 == 0 ? -1f : 1f;
                offset.Z *= Helper.Random % 2 == 0 ? -1f : 1f;

                Vector3 dir = offset;
                dir.Normalize();

                Explosion.Emit(this, offset, dir, 1f);
            }
        }

        void EndDeath()
        {
            isdead = true;
            Body.Visible = false;
        }

        LitObject body;
        public LitObject Body
        {
            get
            {
                return body;
            }
        }
        public ChiseDrive.Graphics.SkinnedModel.AnimationPlayer AnimationPlayer
        {
            get
            {
                return Body.AnimationPlayer;
            }
        }


        public Nullable<Vector4> Intersects(Ray test)
        {
            if (components.Count > 0)
            {
                Nullable<Vector4> retvalue;
                foreach (Body body in components)
                {
                    retvalue = body.Intersects(test);
                    if (retvalue != null) return retvalue;
                }
            }
            return (Body != null) ? Body.Intersects(test) : null;
        }
        public Nullable<Vector4> Intersects(BoundingBox test)
        {
            if (components.Count > 0)
            {
                Nullable<Vector4> retvalue;
                foreach (Body body in components)
                {
                    retvalue = body.Intersects(test);
                    if (retvalue != null) return retvalue;
                }
            }
            return (Body != null) ? Body.Intersects(test) : null;
        }
        public Nullable<Vector4> Intersects(BoundingSphere test)
        {
            if (components.Count > 0)
            {
                Nullable<Vector4> retvalue;
                foreach (Body body in components)
                {
                    retvalue = body.Intersects(test);
                    if (retvalue != null) return retvalue;
                }
            }
            return (Body != null) ? Body.Intersects(test) : null;
        }
        public Nullable<Vector4> Intersects(IBounding test)
        {
            if (components.Count > 0)
            {
                Nullable<Vector4> retvalue;
                foreach (Body body in components)
                {
                    retvalue = body.Intersects(test);
                    if (retvalue != null) return retvalue;
                }
            }
            return (Body != null) ? Body.Intersects(test) : null;
        }
        public BoundingBox BoundingBox
        {
            get { return BoundingBox.CreateFromSphere(BoundingSphere); }
        }
        public virtual BoundingSphere BoundingSphere
        {
            get { return new BoundingSphere(Position, CollisionRadius); }
        }
    }
}