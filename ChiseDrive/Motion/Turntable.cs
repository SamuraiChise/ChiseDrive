using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ChiseDrive;
using ChiseDrive.Units;
using ChiseDrive.Motion;

namespace ModelPreviewer
{
    public class Turntable : Locomotion
    {
        float angle;

        public Turntable(Unit owner)
            : base(owner)
        {
            angle = 9f;
        }

        public override Locomotion Clone(UnitEX owner)
        {
            return new Turntable(follow);
        }

        public override void Update(Time elapsed)
        {
            if (Body != null)
            {
                angle = 9f;

                follow.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Forward, angle);
                Body.RotationPosition = Matrix.CreateFromQuaternion(follow.Rotation) * Matrix.CreateTranslation(follow.Position);
            }
        }
    }
}