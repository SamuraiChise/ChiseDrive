using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ChiseDrive.Storage
{
    interface IStorable
    {
        void Load(Stream input);
        void Save(Stream output);
    }
}
