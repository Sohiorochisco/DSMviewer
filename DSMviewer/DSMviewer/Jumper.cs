using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class Jumper : Switch
    {
        
        public Jumper(DataRow switchData) : base(switchData) { }
    }
}
