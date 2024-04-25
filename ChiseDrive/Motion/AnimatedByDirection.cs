using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ChiseDrive.Graphics.SkinnedModel;

namespace ChiseDrive.Motion
{
    class AnimatedByDirection : Locomotion
    {
        float rotation = 0f;

        public AnimatedByDirection(Units.Unit owner) : base(owner)
        {
        }

        public override Locomotion Clone(Units.UnitEX owner)
        {
            return new AnimatedByDirection(follow);
        }

        Vector3 axis = Vector3.Backward;
        public override void Update(Time elapsed)
        {
            if (Physics != null)
            {
                if (Vector3.Distance(follow.Position, destination) > Physics.DecelerationRate)
                {
                    Vector3 delta = destination - follow.Position;
                    delta = Helper.Normalize(delta);
                    delta *= AccelerationRate;
                    Physics.Push(delta);

                    follow.Rotation = Helper.RotateToFaceQuaternion(follow.Position, destination, Vector3.Backward);
                }

                {
                    Vector3 origin = follow.Position;

                    follow.Position = Physics.RunPhysics(elapsed, follow.Position);
                    follow.Position = follow.Game.World.CorrectForHeight(follow.Position);

                    Vector3 delta = origin - follow.Position;
                    delta.Z = 0f;
                    if (delta != Vector3.Zero)
                    {
                        delta.Normalize();
                        rotation = (float)Math.Atan2((double)delta.Y, (double)delta.X);
                    }
                }
            }

            if (Body != null)
            {
                // TODO: Remove when better art is built!
                Vector3 bodyposition = follow.Position;
                bodyposition.Z += 4f;

                Body.RotationPosition = Matrix.CreateFromAxisAngle(axis, rotation) * Matrix.CreateTranslation(bodyposition);
                //Body.PositionRotation = Matrix.CreateFromQuaternion(follow.Rotation) * Matrix.CreateTranslation(bodyposition);
            }
        }
    }
}