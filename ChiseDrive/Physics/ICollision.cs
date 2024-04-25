using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChiseDrive.Units;

namespace ChiseDrive.Physics
{
    interface ICollision
    {
        ID ID { get; }
        IPhysics Physics { get; }
        IBounding Bounding { get; }
    }
}
