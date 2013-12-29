using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class ProtectedSwitch : Switch
    {
         
         public ProtectedSwitch(DataRow switchData) : base(switchData) { }
    }
}
