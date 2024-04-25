using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ChiseDrive.Motion
{
    public class SimpleThrust : Locomotion
    {
        public float SettleDistance = 5f;

        public SimpleThrust(Units.Unit owner)
            : base(owner)
        {
        }

        public override Locomotion Clone(Units.UnitEX owner)
        {
            return new SimpleThrust(follow);
        }

        public override void Update(Time elapsed)
        {
            if (Physics != null)
            {
                actualthrottle = 0f;

                if (Throttle > 0f)
                {
                    Vector3 direction = destination - follow.Position;

                    float distance = Math.Abs(direction.Length());
                    //Debug.DebugText.Write("Distance to Settle Point: " + distance);

                    if (distance < SettleDistance)
                    {
                        destination = follow.Position;
                    }

                    if (destination != follow.Position)
                    {
                        Helper.Normalize(ref direction);

                        Quaternion desiredrotation = Helper.RotateToFaceQuaternion(follow.Position, destination, follow.RotationPosition.Up);
                        Quaternion shiprotation = follow.Rotation;

                        Quaternion newrotation = Quaternion.Lerp(shiprotation, desiredrotation, TurnRate);

                        direction = Vector3.Transform(Vector3.Backward, newrotation);

                        //Vector3 shipdirection = Vector3.Transform(Vector3.Forward, follow.Rotation);

                        //direction = Helper.EaseTo(shipdirection, direction, TurnRate, elapsed);
                        //Helper.Normalize(ref direction);
                        follow.Rotation = newrotation;

                        float availablethrust = AccelerationRate - Physics.DecelerationRate;
                        float appliedthrust = Physics.DecelerationRate + availablethrust * Throttle;

                        //Physics.TerminalVelocity = MaxSpeed * Throttle;

                        Vector3 push = direction * appliedthrust * elapsed;
                        actualthrottle = Throttle;
                        Physics.Push(push);
                    }
                }

                follow.Position = Physics.RunPhysics(elapsed, follow.Position);
            }

            if (Body != null)
            {
                Body.RotationPosition = follow.RotationPosition;
            }
        }
    }
}