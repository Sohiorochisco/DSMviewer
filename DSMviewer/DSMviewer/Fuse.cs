using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class Fuse : Switch
    {
        
        public Fuse(DataRow switchData) : base(switchData) { }
    }
}
