using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Graphics
{
    public interface ILitObject
    {
        void AddLight(PointLight light);
        void RemoveLight(PointLight light);
    }
}
