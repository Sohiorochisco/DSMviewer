using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class Sectionalizer : Switch
    {
        
        public Sectionalizer(DataRow switchData) : base(switchData) { }
    }
}
