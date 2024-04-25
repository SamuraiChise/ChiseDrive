using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ChiseDrive.Graphics;
using ChiseDrive.Motion;
using ChiseDrive.Particles;
using ChiseDrive.Physics;
using ChiseDrive.Pathfinding;

namespace ChiseDrive.Units
{
    public sealed class UnitEX : IDisposable, ILitObject, IFollow, IBounding
    {
        public enum UnitStatus
        {
            None,
            Alive,
            Dying,
            Dead,
            Preview
        };

        // Value Types
        UnitStatus status;
        public UnitStatus Status 
        {
            get { return status; }
            set
            {
                status = value;
                bool visible = status == UnitStatus.None ? false : true;
                Visible = visible;
            }
        }
        bool Visible
        {
            get
            {
                return Body.Visible;
            }
            set
            {
                Body.Visible = value;
                foreach (Addon addon in Addons)
                {
                    addon.Visible = value;
                }
            }
        }

        public ID ID { get; set; }
        public bool Active { get { return Status == UnitStatus.None ? false : true; } }
        Node[] Occupancy;

        #region IFollow
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Matrix RotationPosition { get { return Body != null ? Body.RotationPosition : Matrix.Identity; } }
        public float Scale { get { return scale; } }
        public Vector3 Velocity { get { return Physics != null ? Physics.Velocity : Vector3.Zero; } }
        float scale = 0f;
        #endregion

        #region IBounding
        public Nullable<Vector4> Intersects(Ray test, float length)
        {
            throw new NotImplementedException();
        }
        public Nullable<Vector4> Intersects(BoundingBox test)
        {
            throw new NotImplementedException();
        }
        public Nullable<Vector4> Intersects(BoundingSphere test)
        {
            return Body.Intersects(test);
        }
        public Nullable<Vector4> Intersects(IBounding test)
        {
            throw new NotImplementedException();
        }

        public float BoundingRadius { get; set; }
        Vector3 BoundingSize
        {
            get
            {
                return new Vector3(BoundingRadius, BoundingRadius, BoundingRadius);
            }
        }
        public BoundingSphere BoundingSphere
        {
            get
            {
                return new BoundingSphere(Position, BoundingRadius);
            }
        }
        public BoundingBox BoundingBox
        {
            get
            {
                return new BoundingBox(Position - BoundingSize, Position + BoundingSize);
            }
        }
        #endregion

        // Reference Types
        public LitObject Body { get; set; }
        public Locomotion Locomotion { get; set; }
        public IPhysics Physics { get; set; }
        public Faction Faction { get; set; }

        ActionSet actions;
        public ActionSet ActionSet 
        {
            get
            {
                return actions;
            }
            set
            {
                actions = value;
                actions.SetAnimationPlayer(Body.AnimationPlayer);
            }
        }

        Attributes attributes;
        public Attributes Attributes
        {
            get
            {
                return attributes;
            }
            set
            {
                attributes = value;
                if (Physics != null)
                {
                    Physics.DecelerationRate = attributes.Deceleration;
                    Physics.Mass = attributes.Mass;
                    Physics.TerminalVelocity = attributes.TerminalVelocity;
                }
                if (Locomotion != null)
                {
                    Locomotion.SetAttributes(attributes);
                }
            }
        }

        public ChiseDriveGame Game { get; set; }

        // Arrays/Lists
        UnitEffect[] UnitEventEffects = new UnitEffect[(int)EventType.Count];
        public List<AttachmentPoint> Attachments = new List<AttachmentPoint>();
        public List<Addon> Addons = new List<Addon>();

        // Speech Data
        Cue SpeechCue { get; set; }
        SoundBank SoundBank { get; set; }
        AudioEmitter audioEmitter = new AudioEmitter();
        Cooldown VoiceCooldown = new Cooldown(Time.FromSeconds(5f));

        UnitEX() { } // Forbidden!!!

        /// <summary>
        /// Creates a unit based off a settings file.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="settings"></param>
        public UnitEX(ChiseDriveGame game, UnitSettingsEX settings)
        {
            this.Game = game;
            this.Game.UnitsEX.Add(this);

            this.Attributes = settings.Attributes.Clone();
            this.Body = settings.MainBody.BuildLitObject(game);

            this.ActionSet = Game.Content.Load<ActionSet>(settings.ActionFile).Clone();

            this.scale = settings.MainBody.Scale;
            BoundingRadius = scale;

            this.Faction = new Faction("Unknown");

            this.ID = ID.Generate();

            BuildAttachments();

            foreach (AddonSettings addonSettings in settings.Addons)
            {
                BuildAddonFromSettings(addonSettings);
            }

            CacheObjectLights();

            Occupancy = new Node[1];
            Occupancy[0] = Node.Invalid;
        }

        /// <summary>
        /// Clones a Unit
        /// </summary>
        /// <returns></returns>
        public UnitEX Clone()
        {
            UnitEX clone = new UnitEX();

            clone.Game = this.Game;
            clone.Game.UnitsEX.Add(clone);
            clone.Attributes = this.Attributes.Clone();
            clone.Body = this.Body.Clone();
            clone.BoundingRadius = this.BoundingRadius;

            clone.ActionSet = this.ActionSet.Clone();

            clone.ID = ID.Generate();

            clone.scale = this.scale;

            clone.BuildAttachments();

            foreach (Addon add in Addons)
            {
                string rootName = add.AttachmentName;
                AttachmentPoint root = null;
                foreach (AttachmentPoint p in Attachments)
                {
                    if (p.Name == rootName)
                        root = p;
                }
                if (root == null) throw new Exception("Unable to clone the root.");
                clone.Addons.Add(add.Clone(root));
            }

            clone.Faction = this.Faction;

            clone.CacheObjectLights();

            clone.Locomotion = this.Locomotion.Clone(clone);
            clone.Physics = this.Physics.Clone();

            clone.Occupancy = new Node[1];
            clone.Occupancy[0] = Node.Invalid;

            return clone;
        }

        /// <summary>
        /// Builds an addon from a settings file.
        /// </summary>
        /// <param name="settings"></param>
        public void BuildAddonFromSettings(AddonSettings settings)
        {
            AttachmentPoint root = null;
            foreach (AttachmentPoint p in Attachments)
            {
                if (p.Name == settings.ParentBoneName)
                    root = p;
            }
            if (root == null) throw new Exception("Unable to clone the root.");
            
            Addon addon = null;
            
            switch (settings.AddonSystemName)
            {
                case "BladeWeapon":
                    addon = new BladeWeapon(Game, settings);
                    break;
            }

            Addons.Add(addon.Clone(root));
        }

        /// <summary>
        /// Applies the addon to the Unit, replacing an existing addon at that attachment.
        /// </summary>
        /// <param name="addon"></param>
        public void SetReplaceAddon(Addon addon)
        {
            foreach (Addon add in Addons.FindAll(delegate(Addon a) { return a.AttachmentName == addon.AttachmentName; }))
            {
                add.Visible = false;
            }
            Addons.RemoveAll(delegate(Addon a) { return a.AttachmentName == addon.AttachmentName; });
            foreach (AttachmentPoint root in Attachments)
            {
                if (root.Name == addon.AttachmentName)
                {
                    Addons.Add(addon.Clone(root));
                }
            }
            CacheObjectLights();

            // Update the visibility
            Visible = Body.Visible;

            foreach (Addon add in Addons)
            {
                add.Visible = Body.Visible;
            }
        }

        public void ClearAddons()
        {
            foreach (Addon add in Addons)
            {
                add.Visible = false;
            }
            
            Addons.Clear();
        }

        /// <summary>
        /// Disposes the Unit and it's resources.
        /// </summary>
        public void Dispose()
        {
            // Dispose Assets
            if (Locomotion != null) Locomotion.Dispose();
            Locomotion = null;

            foreach (AttachmentPoint point in Attachments)
            {
                point.Dispose();
            }
            Attachments.Clear();

            foreach (Addon addon in Addons)
            {
                addon.Dispose();
            }
            Addons.Clear();

            if (Body != null) Body.Dispose();
            Body = null;
            Physics = null;

            Game.UnitsEX.Remove(this);
        }

        #region ILitObject
        /// <summary>
        /// Retrieves lights from the addons and applies them to all the available LitObjects
        /// </summary>
        public void CacheObjectLights()
        {
            Body.ClearLights();
            List<PointLight> lights = new List<PointLight>();

            foreach (Addon addon in Addons)
            {
                if (addon is ILightEmitter)
                {
                    // Finds all the lights on the addons of this object
                    lights.AddRange((addon as ILightEmitter).Lights);
                }
            }

            foreach (PointLight light in lights)
            {
                // Caches the lights on the bodies of this object
                AddLight(light);
            }
        }

        /// <summary>
        /// Adds a light to the LitObject(s)
        /// </summary>
        /// <param name="light"></param>
        public void AddLight(PointLight light)
        {
            if (Body != null) Body.AddLight(light);
            foreach (Addon addon in Addons)
            {
                PhysicalAddon pa = addon as PhysicalAddon;
                if (pa != null && pa.Body != null)
                {
                    pa.Body.AddLight(light);
                }
            }
        }

        /// <summary>
        /// Removes a light from the LitObject(s)
        /// </summary>
        /// <param name="light"></param>
        public void RemoveLight(PointLight light)
        {
            if (Body != null) Body.RemoveLight(light);
            foreach (Addon addon in Addons)
            {
                PhysicalAddon pa = addon as PhysicalAddon;
                if (pa != null)
                {
                    pa.Body.RemoveLight(light);
                }
            }
        }
        #endregion

        /// <summary>
        /// Build the attachment list for the Unit
        /// </summary>
        void BuildAttachments()
        {
            if (Body != null)
            {
                List<String> names = Body.GetBoneNames();

                if (names != null)
                {
                    foreach (String name in names)
                    {
                        Attachments.Add(new AttachmentPoint(name, Body, this));
                    }
                }
            }
        }

        /// <summary>
        /// Fully feature re-position that automatically corrects 
        /// for world terrain and occupies space in the GameBoard
        /// </summary>
        /// <param name="newposition"></param>
        public void SetPosition(Vector3 newposition, bool airborne)
        {
            if (Game.World != null)
            {
                newposition = Game.World.CorrectForBounds(newposition);
                Vector3 heightcorrect = Game.World.CorrectForHeight(newposition);

                if (!airborne) newposition = heightcorrect;
                else if (newposition.Z < heightcorrect.Z) newposition.Z = heightcorrect.Z;
            }

            Position = newposition;

            if (Game.GameBoard != null)
            {
                Game.GameBoard.TryPlaceObject(Game.GameBoard.WorldToNode(Position), this, ref Occupancy);
            }

            ApplyTransform();
        }

        /// <summary>
        /// Tries to set to the position, and will 'fudge' around collisions.
        /// Only call for initial spawns.
        /// </summary>
        /// <param name="newposition"></param>
        public void TrySetPosition(Vector3 newposition)
        {
            if (Game.World != null)
            {
                newposition = Game.World.CorrectForBounds(newposition);
                newposition = Game.World.CorrectForHeight(newposition);
            }

            foreach (UnitEX unit in Game.UnitsEX)
            {
                if (InCollision(unit))
                {
                    Vector3 direction = unit.Position - Position;

                    float delta = direction.Length();
                    if (direction != Vector3.Zero) Helper.Normalize(direction);

                    delta -= unit.BoundingSphere.Radius + BoundingSphere.Radius;

                    direction *= delta;
                    newposition -= direction;
                }
            }

            Position = newposition;

            if (Game.GameBoard != null)
            {
                Game.GameBoard.TryPlaceObject(Game.GameBoard.WorldToNode(Position), this, ref Occupancy);
            }

            ApplyTransform();
        }

        /// <summary>
        /// Returns true if the units are in collision.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public bool InCollision(UnitEX unit)
        {
            if (!Active) return false;
            if (!unit.Active) return false;
            if (Status == UnitStatus.Dead) return false;
            if (unit.Status == UnitStatus.Dead) return false;
            if (ID == unit.ID) return false;

            if (BoundingSphere.Intersects(unit.BoundingSphere)) 
                return true;
            return false;
        }

        /// <summary>
        /// Builds the position rotation matrix.  Use SetPosition
        /// if attempting to update simulation information.
        /// </summary>
        public void ApplyTransform()
        {
            if (Body != null)
            {
                Body.RotationPosition = Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);
            }
        }

        /// <summary>
        /// Processes an event for the Unit.  Please see EventSystem.Events for
        /// a list of possible events.
        /// </summary>
        /// <param name="e">The reference for this particular event ie: TakeDamage, TakeHealing</param>
        /// <param name="value">The value/intensity for the event</param>
        /// <param name="source">The ID of the source for this event (can be None)</param>
        public void ProcessEvent(UnitEvent e)
        {
            if (e.Type == EventType.Count) throw new Exception("Unable to process event Count");

            if (UnitEventEffects[e] != null) UnitEventEffects[e].TriggerAt(e.Location);

            switch (e.Type)
            {
                case EventType.TakeDamage: TakeDamage(e.Value, e.Instigator); break;
                case EventType.TakeHealing: TakeHealing(e.Value, e.Instigator); break;
                case EventType.Collision: break;// TakeDamage(value, source); break;
                case EventType.StartDeath: StartDeath(e.Value, e.Instigator); break;
                case EventType.ContinueDeath: break;
                case EventType.FinishDeath: FinishDeath(e.Value, e.Instigator); break;
                case EventType.Alive: break;
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The unit takes healing equal to the value.  CurrentHealth will not
        /// exceed MaximumHealth.  Check a unit's status before applying to avoid
        /// undesired reincarnations.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="source"></param>
        void TakeHealing(float value, ID source)
        {
            if (Status == UnitStatus.Dead) Status = UnitStatus.Alive;
            if (Status == UnitStatus.Dying) Status = UnitStatus.Alive;

            Attributes.CurrentHealth += value;
            if (Attributes.CurrentHealth > Attributes.MaximumHealth) Attributes.CurrentHealth = Attributes.MaximumHealth;
        }

        /// <summary>
        /// The unit takes damage equal to the value.  Only applies to Alive units.
        /// CurrentHealth will not go below 0.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="source"></param>
        void TakeDamage(float value, ID source)
        {
            if (Status != UnitStatus.Alive) return;//No damage for dead people!

            Attributes.CurrentHealth -= value;
            if (Attributes.CurrentHealth <= 0f)//Start a death operation
            {
                Attributes.CurrentHealth = 0f;
                UnitEvent e = UnitEvent.Announce(EventType.StartDeath, source, ID);
                ProcessEvent(e);
            }
        }

        /// <summary>
        /// Starts the character's death.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="source"></param>
        void StartDeath(float value, ID source)
        {
            Status = UnitStatus.Dying;

            if (Body != null && Body.AnimationPlayer != null)
            {
                if (ActionSet != null)
                {
                    ActionSet.StartAction("Death");
                    ActionSet.SetPassiveAction("");
                }
            }
        }

        /// <summary>
        /// Finishes killing off a unit, leaving them in the dead condition.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="source"></param>
        void FinishDeath(float value, ID source)
        {
            Status = UnitStatus.Dead;
        }

        /// <summary>
        /// Updates the Unit, performing necessary work for the Unit to function.
        /// </summary>
        /// <param name="elapsed">The time elapsed since the last update.</param>
        public void Update(Time elapsed)
        {
            audioEmitter.Position = Position;

            if (Status == UnitStatus.Alive)
            {
                ProcessEvent(UnitEvent.Announce(EventType.Alive, ID, ID.None, 1f, Position));
            }
            else if (Status == UnitStatus.Dying)
            {
                if (ActionSet.GetCurrentAction() == null)
                {
                    ProcessEvent(UnitEvent.Announce(EventType.FinishDeath, ID.None, ID));
                }
                else
                {
                    ProcessEvent(UnitEvent.Announce(EventType.ContinueDeath, ID.None, ID));
                }
            }

            if (Status != UnitStatus.Dead && Status != UnitStatus.None)
            {
                if (Locomotion != null && Status != UnitStatus.Preview) Locomotion.Update(elapsed);

                if (ActionSet != null) ActionSet.Update(elapsed);

                foreach (Addon addon in Addons)
                {
                    addon.Update(elapsed);
                }
            }
        }
    }
}