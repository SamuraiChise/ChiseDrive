using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ChiseDrive.Motion
{
    public class Direct : Locomotion
    {
        public Direct(ChiseDrive.Units.Unit owner)
            : base(owner)
        {
            owner.Physics = new ChiseDrive.Physics.NoPhysics();
        }

        public override Locomotion Clone(Units.UnitEX owner)
        {
            return new Direct(follow);
        }

        public Quaternion Facing { get; set; }
        public bool FlipY { get; set; }
        float rotation = 0f;
        const float rotationoffset = 0f;//(float)Math.PI / 2f;

        public override void Update(Time elapsed)
        {
            if (follow.Position != destination)
            {
                Vector3 delta = destination - follow.Position;

                float distance = delta.Length();
                if (distance > MaxSpeed * elapsed)
                {
                    delta.Normalize();
                    delta *= MaxSpeed * elapsed;
                }

                //delta *= elapsed;
                Vector3 newdestination = follow.Position + delta;

                float newrotation = delta.Y * (FlipY ? -1f : 1f);
                Helper.Clamp(ref newrotation, -0.5f, 0.5f);
                Helper.EaseTo(ref rotation, newrotation, 0.05f, elapsed);

                //follow.Rotation = Quaternion.CreateFromAxisAngle(Axis, delta.Y);
                follow.Rotation = Facing * Quaternion.CreateFromAxisAngle(Vector3.Backward, rotationoffset + rotation);

                follow.Position = follow.Physics.RunPhysics(elapsed, newdestination);
                follow.ApplyTransform();
            }
        }
    }
}