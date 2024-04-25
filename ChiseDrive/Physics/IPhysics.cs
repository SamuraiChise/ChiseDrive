using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace ChiseDrive.Physics
{
    public interface IPhysics
    {
        float TerminalVelocity { get; set; }
        float DecelerationRate { get; set; }
        float Mass { get; set; }
        Vector3 Velocity { get; }
        float Speed { get; }

        bool ApplyGravity { get; set; }
        IPhysics Clone();

        void Accelerate(Vector3 origin, Vector3 position, float acceleration, Time duration);
        void Accelerate(Vector3 acceleration, Time duration);

        void Push(Vector3 origin, Vector3 position, float force);
        void Push(Vector3 force);

        void Collision(Vector3 newvelocity, Time withoutdampen);

        void Dampen(float decelleration);

        /// <summary>
        /// Force a complete immediate stop.  Not good for simulation,
        /// but good for teleporting units.
        /// </summary>
        void KillAllForces();

        /// <summary>
        /// Moves a position according to the physics configuration.
        /// </summary>
        /// <param name="elapsed"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        Vector3 RunPhysics(Time elapsed, Vector3 position);
        void RunPhysics(Time elapsed, ref Vector3 position);
    }
}
