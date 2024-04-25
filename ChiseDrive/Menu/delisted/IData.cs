using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Menu
{
    interface IData
    {
        string DataKey {get; set;}
        void WriteData(ref Dictionary<String, String> data);
        void ReadData(Dictionary<String, String> data);
    }
}
