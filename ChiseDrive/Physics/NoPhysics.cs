using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace ChiseDrive.Physics
{
    public class NoPhysics : IPhysics
    {
        #region Fields
        Vector3 velocity = Vector3.Zero;
        Vector3 acceleration = Vector3.Zero;
        Vector3 lastposition = Vector3.Zero;
        Timer accelerationtime = new Timer(Time.Zero);
        Timer collisiontime = new Timer(Time.Zero);
        float dampenrate = 0.5f;
        float mass = 1.0f;
        float terminalvelocity = 1.0f;
        #endregion

        #region Properties
        public Vector3 Velocity 
        {
            get 
            { 
                return velocity; 
            }
        }
        public float Speed 
        {
            get 
            { 
                return velocity.Length(); 
            } 
        }
        public virtual float TerminalVelocity 
        {
            get
            { 
                return terminalvelocity; 
            } 
            set 
            { 
                terminalvelocity = value; 
            } 
        }
        public float DecelerationRate 
        {
            get 
            { 
                return dampenrate; 
            } 
            set 
            { 
                dampenrate = value; 
            } 
        }
        public float Mass 
        {
            get
            {
                return mass;
            }
            set
            {
                mass = value;
            }
        }
        #endregion

        public bool ApplyGravity { get; set; }

        /// <summary>
        /// Nothing to do.
        /// </summary>
        public NoPhysics() { }
        public IPhysics Clone()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This will replace any current acceleration on the target.
        /// In an ideal simulation, there would be a list of accelerations
        /// on the target that would all apply over time, however, this
        /// is a game.
        /// </summary>
        /// <param name="acceleration">The change in velocity.</param>
        /// <param name="duration">The time to apply it in.</param>
        /// <param name="origin">The place that the acceleration emits from.</param>
        public void Accelerate(Vector3 origin, Vector3 position, float acceleration, Time duration)
        {
        }

        /// <summary>
        /// This will replace any current acceleration on the target.
        /// In an ideal simulation, there would be a list of accelerations
        /// on the target that would all apply over time, however, this
        /// is a game.
        /// </summary>
        /// <param name="acceleration">The change in velocity.</param>
        /// <param name="duration">The time to apply it in.</param>
        public void Accelerate(Vector3 acceleration, Time duration)
        {
        }

        /// <summary>
        /// Applies force to the object.  The force must be greater than
        /// the object's mass or else the object will not move.
        /// </summary>
        /// <param name="origin">The place that the force emits from.</param>
        /// <param name="force">The ammount of force to apply.</param>
        public void Push(Vector3 origin, Vector3 position, float force)
        {
        }

        /// <summary>
        /// Applies force to the object.  The force must be greater than
        /// the object's mass or else the object will not move.
        /// </summary>
        /// <param name="force">The ammount of force to apply.</param>
        public void Push(Vector3 force)
        {
        }

        public void Collision(Vector3 newvelocity, Time collisiontime)
        {
        }

        /// <summary>
        /// Good for situations where an object must come to a complete
        /// stop.  Not very realistic.
        /// </summary>
        public void KillAllForces()
        {
            acceleration = Vector3.Zero;
            accelerationtime.Set(Time.Zero);
            velocity = Vector3.Zero;
        }

        /// <summary>
        /// Applies any existing acceleration to the object.
        /// </summary>
        /// <param name="elapsed">Time elapsed.</param>
        void ApplyAcceleration(Time elapsed)
        {
        }

        /// <summary>
        /// Applies velocity to the object's position.
        /// </summary>
        /// <param name="elapsed"></param>
        Vector3 ApplyVelocity(Time elapsed, Vector3 position)
        {
            return Vector3.Zero;
        }

        /// <summary>
        /// Dampens forces on the object.  This both keeps
        /// a reasonable maximum speed, and slows objects over time.
        /// This is completely not realistic for space.
        /// </summary>
        /// <param name="decelleration"></param>
        public void Dampen(float decelleration)
        {
        }

        public Vector3 RunPhysics(Time elapsed, Vector3 position)
        {
            velocity = position - lastposition;
            lastposition = position;
            return position;
        }

        public void RunPhysics(Time elapsed, ref Vector3 position)
        {
            velocity = position - lastposition;
            lastposition = position;
        }
    }
}
