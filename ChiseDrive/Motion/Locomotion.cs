using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ChiseDrive.Physics;
using ChiseDrive.Units;
using ChiseDrive.Graphics;
using ChiseDrive.World;
using ChiseDrive.Pathfinding;

namespace ChiseDrive.Motion
{
    /// <summary>
    /// Locomotion manages how a body gets from point A to point B.
    /// Does it use animations?  Or particle effects?  Or just gets there?
    /// </summary>
    public abstract class Locomotion : IDisposable
    {
        public float TurnRate
        {
            get
            {
                return turnrate;
            }
            set
            {
                turnrate = value;
            }
        }
        public float AccelerationRate
        {
            get
            {
                return moverate;
            }
            set
            {
                moverate = value;
            }
        }
        public float MaxSpeed
        {
            get
            {
                return maxspeed;
            }
            set
            {
                maxspeed = value;
            }
        }
        
        public float Throttle
        {
            get
            {
                return throttle;
            }
            set
            {
                throttle = value;
                Helper.Clamp(ref throttle, 0f, 1f);
            }
        }
        public float ActualThrottle
        {
            get
            {
                return actualthrottle;
            }
        }

        public float SettleDistance
        {
            get { return settledistance; }
        }

        public Vector3 Position
        {
            get
            {
                return followEX != null ? followEX.Position : follow != null ? follow.Position : Vector3.Zero;
            }
            set
            {
                followEX.Position = value;
            }
        }

        public bool InCollision { get; set; }

        float turnrate;
        float moverate;
        float throttle;
        float settledistance;
        protected float actualthrottle;
        float maxspeed;

        Path path;
        public Path Path
        {
            get
            {
                return path;
            }
        }
        public GameBoard GameBoard
        {
            get
            {
                return followEX != null ? followEX.Game.GameBoard : follow.Game.GameBoard;
            }
        }

        protected Units.Unit follow;
        protected Units.UnitEX followEX;

        protected Vector3 destination;
        protected Vector3 facing;

        public LitObject Body
        {
            get 
            {
                return follow != null ? follow.Body : followEX != null ? followEX.Body : null;
            }
        }
        public IPhysics Physics
        {
            get 
            {
                return follow != null ? follow.Physics : followEX != null ? followEX.Physics : null;
            }
        }

        public Locomotion(Unit owner)
        {
            InCollision = false;
            path = new Path(owner.Game);
            follow = owner;
        }

        public Locomotion(UnitEX owner)
        {
            InCollision = false;
            path = new Path(owner.Game);
            followEX = owner;

            moverate = owner.Attributes.Acceleration;
            turnrate = owner.Attributes.TurnSpeed;
            maxspeed = owner.Attributes.TerminalVelocity;

            settledistance = owner.Attributes.Deceleration;
            Throttle = 1f;
        }

        protected Locomotion(Locomotion copy, UnitEX owner)
        {
            this.turnrate = copy.turnrate;
            this.moverate = copy.moverate;
            this.throttle = copy.throttle;
            this.settledistance = copy.settledistance;
            this.actualthrottle = copy.actualthrottle;
            this.maxspeed = copy.maxspeed;
            this.follow = null;
            this.followEX = owner;

            this.path = new Path(follow != null ? follow.Game : followEX.Game);
        }
        public abstract Locomotion Clone(UnitEX owner);

        public void SetAttributes(Attributes attributes)
        {
            turnrate = attributes.TurnSpeed;
            moverate = attributes.Acceleration;
            maxspeed = attributes.TerminalVelocity;
            settledistance = attributes.Deceleration;
        }

        public virtual void Dispose()
        {
            follow = null;
            path.Dispose();
            path = null;
        }

        /// <summary>
        /// Applies a Physics push to the object.
        /// </summary>
        /// <param name="destination">The place to push towards.</param>
        public void PushTo(Vector3 destination)
        {
            if (Physics == null) throw new Exception("Physics required to Push a Locomotion object.");

            Vector3 delta = destination - Position;
            if (delta == Vector3.Zero) throw new Exception("Unable to Push, destination matches Position.");

            Helper.Normalize(ref delta);
            delta *= AccelerationRate;
            Physics.Push(delta);
        }

        public void CorrectForWorld(ref Vector3 position, bool checkHeight)
        {
            position = followEX.Game.World.CorrectForBounds(position);

            if (checkHeight)
            {
                position = followEX.Game.World.CorrectForHeight(position);
            }
        }

        public void UpdateFacing(Vector3 direction)
        {
            direction.Z = 0f;

            if (direction != Vector3.Zero)
            {
                direction.Normalize();

                float rotation = (float)Math.Atan2((double)-direction.X, (double)direction.Y);
                
                Quaternion destination = Quaternion.CreateFromAxisAngle(Vector3.Backward, rotation);
                followEX.Rotation = Quaternion.Lerp(followEX.Rotation, destination, turnrate);
            }
        }

        public void MoveTo(Vector3 destination)
        {
            MoveWithin(destination, 1f);
        }

        public void Stop()
        {
            MoveTo(follow.Position);
        }

        public virtual void MoveWithin(Vector3 destination, float distance)
        {
            InCollision = false;
            this.destination = destination;
            this.settledistance = distance;
        }

        public virtual void RotateToFace(Vector3 destination)
        {
            this.facing = destination;
        }

        public abstract void Update(Time elapsed);
    }
}