using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.World
{
    class WorldNode
    {
        public bool Footprints = false;
        public List<object> Occupancy = null;
        public float Height = 0f;
    }
}