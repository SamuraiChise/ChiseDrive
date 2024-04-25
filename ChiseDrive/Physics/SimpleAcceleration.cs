using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ChiseDrive.Physics
{
    class SimpleAcceleration
    {
        Vector3 position = Vector3.Zero;
        Vector3 weightedAcceleration = Vector3.Zero;
        Vector3 newAcceleration = Vector3.Zero;
        float accelerationWeight = FastAcceleration;

        public const float SlowAcceleration = 0.01f;
        public const float FastAcceleration = 0.1f;

        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                weightedAcceleration = Vector3.Zero;
                newAcceleration = Vector3.Zero;
            }
        }
        public float AccelerationType
        {
            set
            {
                accelerationWeight = value;
            }
        }

        public SimpleAcceleration() { }

        public void Push(Vector3 acceleration)
        {
            this.newAcceleration = acceleration;
        }

        public void Update(Time elapsed)
        {
            weightedAcceleration =
                (weightedAcceleration * (1f - accelerationWeight)) 
                + (newAcceleration * accelerationWeight);

            newAcceleration = Vector3.Zero;

            position += weightedAcceleration * elapsed;
        }
    }
}