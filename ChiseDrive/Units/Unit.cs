using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Input;
using ChiseDrive.Physics;
using ChiseDrive.Motion;
using ChiseDrive.Graphics;
using ChiseDrive.Particles;
using ChiseDrive.Pathfinding;

namespace ChiseDrive.Units
{
    public class Unit : IFollow, IDisposable, IDestructible, IBounding, ILitObject, ILightEmitter
    {
        public const string Move = "Move";

        #region Status
        /// <summary>
        /// Setting this to true will get this removed from the Unit
        /// list in the game, and disposed.
        /// </summary>
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
        bool isdead = false;

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

        bool active = false;
        public virtual bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
                if (!active) Visible = false;
            }
        }
        public virtual bool Visible
        {
            get
            {
                return Body != null ? Body.Visible : false;
            }
            set
            {
                if (Body != null) Body.Visible = value;
                foreach (Addon a in addons)
                {
                    a.Visible = value;
                }
            }
        }
        #endregion

        #region IFollow
        // IFollow
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
        public Matrix RotationPosition
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
        public Vector3 Velocity
        {
            get
            {
                if (Physics != null)
                {
                    return Physics.Velocity;
                }
                else
                {
                    return Vector3.Zero;
                }
            }
        }
        #endregion

        #region IDestructible
        // IDestructible
        float health;
        float maxhealth;
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
                if (IsAlive) return 15f + (scale * 0.5f);
                else return 0f;
            }
        }
        List<PointLight> ExplosionLights = new List<PointLight>();
        public void TakeHealing(float value, ID source)
        {
            if (IsDead) isdead = false;
            if (IsDying) deathtime.Set(Time.Zero);

            health += value;

            if (health > maxhealth) health = maxhealth;
        }
        public virtual void TakeDamage(float value, ID source)
        {
            if (!IsAlive) return;
            if (source == ID) return;

            UnitEvent.Announce(EventType.TakeDamage, source, ID, value);

            health -= value;
            if (health <= 0f)
            {
                StartDeath();
                UnitEvent.Announce(EventType.StartDeath, source, ID, value);
            }
        }

        protected Timer deathtime = new Timer(Time.Zero);
        Cooldown newexplosion = new Cooldown(Time.FromSeconds(2f));

        protected virtual void StartDeath()
        {
            deathtime.Set(Time.FromSeconds(scale / 6f));
        }

        public bool DeathByExplosion { get; set; }
        protected virtual void UpdateDeath(Time elapsed)
        {
            deathtime.SubTime(elapsed);
            if (DeathByExplosion) BigExplosion();
            if (deathtime.IsZero) EndDeath();
        }
        protected void BigExplosion()
        {
            for (int i = 0; i < 1+((int)scale%4); i++)
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

        protected virtual void EndDeath()
        {
            isdead = true;
            Visible = false;
            Active = false;
            lights.Clear();
        }
        #endregion

        public Faction Faction { get; set; }

        #region IBounding
        public Nullable<Vector4> Intersects(Ray test, float length)
        {
            if (!Active) return null;
            //float? retvalue = BoundingSphere.Intersects(test);

            //if (retvalue != null)
            {
            //    Vector3 value = test.Position;
            //    value += test.Direction * (float)retvalue;
             //   return new Vector4(value, (float)retvalue);
            }
            //else return null;
            if (scale < 2f)
            {
                float? retvalue = BoundingSphere.Intersects(test);

                if (retvalue != null)
                {
                    Vector3 value = test.Position;
                    value += test.Direction * (float)retvalue;
                    return new Vector4(value, (float)retvalue);
                }
                else return null;
            }
            else if (Body != null)
            {
                return Body.Intersects(test, length);
            }
            else
            {
                return null;
            }
        }
        public Nullable<Vector4> Intersects(BoundingBox test)
        {
            return (Body != null && Active) ? Body.Intersects(test) : null;
        }
        public Nullable<Vector4> Intersects(BoundingSphere test)
        {
            if (!Active) return null;
            //bool retvalue = BoundingSphere.Intersects(test);
            //Vector3 value = BoundingSphere.Center - test.Center;
            //return new Vector4(value + BoundingSphere.Center, value.Length());
            return (Body != null && Active) ? Body.Intersects(test) : null;
        }
        public Nullable<Vector4> Intersects(IBounding test)
        {
            return (Body != null && Active) ? Body.Intersects(test) : null;
        }
        public BoundingBox BoundingBox
        {
            get { return BoundingBox.CreateFromSphere(BoundingSphere); }
        }
        public virtual BoundingSphere BoundingSphere
        {
            get { return new BoundingSphere(Position, CollisionRadius); }
        }
        public float AttackRange { get; set; }
        public float CollisionRadius { get; set; }
        #endregion

        ID id = ID.Generate();
        public ID ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        float locomotionTurnRate = 0f;
        float locomotionAccelerationRate = 0f;

        // Composites
        /// <summary>
        /// Handles moving the unit and body facing/animations.
        /// </summary>
        public Locomotion Locomotion
        {
            get
            {
                return locomotion;
            }
            set
            {
                if (locomotion != null) locomotion.Dispose();
                locomotion = value;
                if (locomotion != null)
                {
                    locomotion.AccelerationRate = locomotionAccelerationRate;
                    locomotion.TurnRate = locomotionTurnRate;
                }
            }
        }

        protected LitObject body;
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

        IPhysics physics;
        public IPhysics Physics
        {
            get
            {
                return physics;
            }
            set
            {
                physics = value;
            }
        }

        List<AttachmentPoint> attachments = new List<AttachmentPoint>();
        public List<AttachmentPoint> AttachmentPoints
        {
            get
            {
                return attachments;
            }
        }
        protected virtual void BuildAttachments(UnitSettings settings)
        {
            if (Body != null)
            {
                List<String> names = Body.GetBoneNames();

                if (names != null)
                {
                    foreach (String name in names)
                    {
                        attachments.Add(new AttachmentPoint(name, Body, this));
                    }
                }
            }

            foreach (UnitSettings.AddonSettings add in settings.Addons)
            {
                ParticleEmitterSettings emitter = null;
                if (add.EmitterSettingsName != null && add.EmitterSettingsName != "") emitter = game.Content.Load<ParticleEmitterSettings>(add.EmitterSettingsName);

                switch (add.AddonSystemName)
                {
                    case "SimpleAttachment":
                        {
                            List<AttachmentPoint> gunAttachments =
                                AttachmentPoint.BuildList(Body, this, add.ParentBoneName);

                            if (add.AddonBody != null)
                            {
                                for (int i = 0; i < gunAttachments.Count; i++)
                                {
                                    LitObject model = new LitObject(game, add.AddonBody);
                                    PhysicalAddon pa = new PhysicalAddon(model, gunAttachments[i]);
                                    pa.BuildAttachments();
                                    Addons.Add(pa);
                                }
                            }
                        }
                        break;

                    case "BulletSystem":
                        {
                            BulletSystem bulletsystem = new BulletSystem(Game);
                            bulletsystem.caliber = Bullet.Caliber.c50mm;
                            List<AttachmentPoint> gunAttachments =
                                AttachmentPoint.BuildList(Body, this, add.ParentBoneName);

                            if (add.AddonBody != null)
                            {
                                // We're using a pass through body, so go ahead and build it and
                                // redirect the emitters to the correct location.

                                List<AttachmentPoint> subAttachments = new List<AttachmentPoint>();

                                for (int i = 0; i < gunAttachments.Count; i++)
                                {
                                    LitObject model = new LitObject(game, add.AddonBody);
                                    PhysicalAddon pa = new PhysicalAddon(model, gunAttachments[i]);
                                    pa.BuildAttachments();
                                    Addons.Add(pa);

                                    subAttachments.AddRange(AttachmentPoint.BuildList(model, this, add.BodyBoneName));
                                }

                                gunAttachments.Clear();
                                gunAttachments = subAttachments;
                            }

                            WeaponSystem weapons = WeaponSystem.TryBuildSystem(gunAttachments, emitter, new Cooldown(Time.FromSeconds(add.Cooldown)), bulletsystem);
                            if (weapons != null)
                            {
                                Systems.Add(weapons);
                                weapons.Name = add.AddonSystemName;
                                weapons.AmmoMax = add.Ammo;
                                weapons.Reload();
                            }
                        }
                        break;
                    case "CannonSystem":
                        {
                            BulletSystem bulletsystem = new BulletSystem(Game);
                            bulletsystem.caliber = Bullet.Caliber.c100mm;

                            List<AttachmentPoint> gunAttachments = 
                                AttachmentPoint.BuildList(Body, this, add.ParentBoneName);

                            if (add.AddonBody != null)
                            {
                                // We're using a pass through body, so go ahead and build it and
                                // redirect the emitters to the correct location.

                                List<AttachmentPoint> subAttachments = new List<AttachmentPoint>();

                                for (int i = 0; i < gunAttachments.Count; i++)
                                {
                                    LitObject model = new LitObject(game, add.AddonBody);
                                    PhysicalAddon pa = new PhysicalAddon(model, gunAttachments[i]);
                                    pa.BuildAttachments();
                                    Addons.Add(pa);

                                    subAttachments.AddRange(AttachmentPoint.BuildList(model, this, add.BodyBoneName));
                                }

                                gunAttachments.Clear();
                                gunAttachments = subAttachments;
                            }

                            WeaponSystem weapons = WeaponSystem.TryBuildSystem(gunAttachments, emitter, new Cooldown(Time.FromSeconds(add.Cooldown)), bulletsystem);
                            if (weapons != null)
                            {
                                Systems.Add(weapons);
                                weapons.Name = add.AddonSystemName;
                                weapons.AmmoMax = add.Ammo;
                                weapons.Reload();
                            }
                        }
                        break;

                    case "LaserSystem":
                        {
                            LaserSystem lasersystem = new LaserSystem(Game);

                            List<AttachmentPoint> gunAttachments =
                                AttachmentPoint.BuildList(Body, this, add.ParentBoneName);

                            if (add.AddonBody != null)
                            {
                                // We're using a pass through body, so go ahead and build it and
                                // redirect the emitters to the correct location.

                                List<AttachmentPoint> subAttachments = new List<AttachmentPoint>();

                                for (int i = 0; i < gunAttachments.Count; i++)
                                {
                                    LitObject model = new LitObject(game, add.AddonBody);
                                    PhysicalAddon pa = new PhysicalAddon(model, gunAttachments[i]);
                                    pa.BuildAttachments();
                                    Addons.Add(pa);

                                    subAttachments.AddRange(AttachmentPoint.BuildList(model, this, add.BodyBoneName));
                                }

                                gunAttachments.Clear();
                                gunAttachments = subAttachments;
                            }

                            WeaponSystem weapons = WeaponSystem.TryBuildSystem(gunAttachments, emitter, new Cooldown(Time.FromSeconds(add.Cooldown)), lasersystem);
                            if (weapons != null)
                            {
                                Systems.Add(weapons);
                                weapons.Name = add.AddonSystemName;
                                weapons.AmmoMax = add.Ammo;
                                weapons.Reload();
                            }
                        }
                        break;
                    case "SimpleEmitter":
                        {
                            VariableEmitSystem system = VariableEmitSystem.TryBuildSystem(add.ParentBoneName, emitter, AttachmentPoints);
                            if (system != null)
                            {
                                system.Active = true;
                                system.Name = add.AddonSystemName;
                                Systems.Add(system);
                            }
                        }
                        break;

                    case "ExpansionSystem":
                        {
                            ExpansionSystem system = ExpansionSystem.TryBuildSystem(add.ParentBoneName, AttachmentPoints);
                            if (system != null)
                            {
                                system.Name = add.AddonSystemName;
                                Systems.Add(system);
                            }
                        }
                        break;

                    case "MissileSystem":
                        {
                            ParticleEmitterSettings missileemitter = game.Content.Load<ParticleEmitterSettings>("Particles/MissileFlashEmitter");
                            ParticleEmitterSettings missileengine = game.Content.Load<ParticleEmitterSettings>("Particles/MissileEngineEmitter");
                            ParticleEmitterSettings missiletrail = game.Content.Load<ParticleEmitterSettings>("Particles/MissileTrailEmitter");

                            // Build the missiles!
                            Model missilebody = game.Content.Load<Model>("Models/missile");
                            Material missilemat = new Material(game.Content, "Units/missilemat");
                            Effect missileeffect = game.Content.Load<Effect>("Effects/ShadedModel");
                            NukeSystem missilesystem = 
                                new NukeSystem(game, missilebody, missilemat, missileeffect, missileengine, missiletrail);

                            List<AttachmentPoint> missileAttachments = AttachmentPoint.TrimList(add.ParentBoneName, AttachmentPoints, 30);

                            WeaponSystem missiles = WeaponSystem.TryBuildSystem(missileAttachments, missileemitter, new Cooldown(Time.FromSeconds(add.Cooldown)), missilesystem);
                            if (missiles != null)
                            {
                                Systems.Add(missiles);
                                missiles.Name = add.AddonSystemName;
                                missiles.AmmoMax = add.Ammo;
                                missiles.Reload();
                            }
                        }
                        break;
                }
            }
        }

        IFollow target;
        public virtual IFollow Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
            }
        }

        List<System> systems = new List<System>();
        public List<System> Systems
        {
            get
            {
                return systems;
            }
        }

        List<Addon> addons = new List<Addon>();
        public List<Addon> Addons
        {
            get
            {
                return addons;
            }
        }

        List<PointLight> lights = new List<PointLight>();
        public List<PointLight> Lights
        {
            get
            {
                return lights;
            }
        }

        // Things the Unit Owns (must Dispose)
        Locomotion locomotion = null;

        // Things the Unit References (no Dispose)
        protected ChiseDriveGame game;
        public ChiseDriveGame Game
        {
            get
            {
                return game;
            }
        }

        // Things the Unit Owns (no Dispose)
        Vector3 position;
        Quaternion rotation;
        float scale;

        public Unit(ChiseDriveGame game)
        {
            Occupancy = new Node[1];
            Occupancy[0] = Node.Invalid;

            this.game = game;
            this.game.Units.Add(this);
        }

        public Unit(ChiseDriveGame game, UnitSettings settings)
        {
            this.game = game;
            this.game.Units.Add(this);
            CreateFromSettings(settings);
        }

        public virtual void CreateFromSettings(UnitSettings settings)
        {
            Model xnaModel = game.Content.Load<Model>(settings.UnitModel.MeshName);
            GameModel chiseDriveModel = new GameModel(xnaModel, settings.UnitModel.Scale);
            Effect effect = game.Content.Load<Effect>(settings.UnitModel.Effect);
            Material material = new Material(Game.Content, settings.UnitModel.Material);
            body = new LitObject(game, chiseDriveModel, material, effect);
            body.Visibility = Visibility.Opaque;
            scale = settings.UnitModel.Scale;

            CollisionRadius = settings.CollisionRadius;
            int hexsize = (int)(CollisionRadius / Game.CollisionAccuracy) + 1;
            int hexcount = 1 + hexsize * 9;
            Occupancy = new Node[hexcount];
            for (int i = 0; i < Occupancy.Length; i++)
            {
                Occupancy[i] = Node.Invalid;
            }

            AttackRange = settings.AttackRange;

            locomotionTurnRate = settings.TurnRate;
            locomotionAccelerationRate = settings.Acceleration;

            // Physics ********************************************************
            Physics = new BasicPhysics();
            Physics.Mass = settings.UnitModel.Scale;
            Physics.DecelerationRate = settings.Deceleration;
            Physics.TerminalVelocity = settings.MaxSpeed;

            // Addons *********************************************************
            BuildAttachments(settings);

            CacheObjectLights();
            HeightCorrect();
        }

        Node[] Occupancy;
        /// <summary>
        /// Applies a unit's position/rotation.
        /// </summary>
        public void ApplyTransform()
        {
            if (Body != null)
                Body.RotationPosition = Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);

            if (Game.GameBoard != null)
                Game.GameBoard.TryPlaceObject(Game.GameBoard.WorldToNode(Position), this, ref Occupancy);
        }

        /// <summary>
        /// Corrects a unit's height based on the world mesh and applies the position/rotation.
        /// </summary>
        public void HeightCorrect()
        {
            if (Game.World != null)
            {
                Position = Game.World.CorrectForHeight(Position);
            }
            ApplyTransform();
        }

        public virtual void Dispose()
        {
            Active = false;

            // Dispose Assets
            if (this.locomotion != null) this.locomotion.Dispose();
            this.locomotion = null;

            // Clear References
            game = null;

            foreach (AttachmentPoint point in attachments)
            {
                point.Dispose();
            }
            attachments.Clear();

            foreach (Addon addon in addons)
            {
                addon.Dispose();
            }
            addons.Clear();

            if (Body != null) body.Dispose();
            body = null;
            Physics = null;
        }

        public void CacheObjectLights()
        {
            Body.ClearLights();
            lights.Clear();

            foreach (Addon addon in addons)
            {
                if (addon is ILightEmitter)
                {
                    // Finds all the lights on the addons of this object
                    lights.AddRange((addon as ILightEmitter).Lights);
                }
            }

            foreach (System system in systems)
            {
                if (system is ILightEmitter)
                {
                    lights.AddRange((system as ILightEmitter).Lights);
                }
            }

            foreach (PointLight light in lights)
            {
                // Caches the lights on the bodies of this object
                AddLight(light);
            }
        }

        public void AddLight(PointLight light)
        {
            if (Body != null) Body.AddLight(light);
            foreach (Addon addon in addons)
            {
                PhysicalAddon pa = addon as PhysicalAddon;
                if (pa != null && pa.Body != null)
                {
                    pa.Body.AddLight(light);
                }
            }
        }

        public void RemoveLight(PointLight light)
        {
            if (Body != null) Body.RemoveLight(light);
            foreach (Addon addon in addons)
            {
                PhysicalAddon pa = addon as PhysicalAddon;
                if (pa != null)
                {
                    pa.Body.RemoveLight(light);
                }
            }
        }

        public virtual void Update(Time elapsed)
        {
            if (IsDying) UpdateDeath(elapsed);

            Debug.Metrics.OpenMetric("Locomotion");
            if (locomotion != null) locomotion.Update(elapsed);
            Debug.Metrics.CloseMetric("Locomotion");

            foreach (Addon addon in addons)
            {
                addon.Update(elapsed);
            }

            foreach (System system in systems)
            {
                system.Update(elapsed);
            }
        }
    }
}