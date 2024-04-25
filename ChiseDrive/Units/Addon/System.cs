using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Units
{
    public abstract class System
    {
        public abstract string Name { get; set; }
        public abstract void Dispose();
        public abstract void Update(Time elapsed);
    }
}