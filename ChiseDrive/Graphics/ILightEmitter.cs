using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Graphics
{
    interface ILightEmitter
    {
        List<PointLight> Lights { get; }
    }
}
