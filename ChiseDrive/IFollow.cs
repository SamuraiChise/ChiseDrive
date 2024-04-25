using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ChiseDrive
{
    public interface IFollow
    {
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
        Matrix PositionRotation { get; }
        float Scale { get; }
        Vector3 Velocity { get; }
    }
}
