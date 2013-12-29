using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class GroundDisconnector : Switch
    {
        
        public GroundDisconnector(DataRow switchData) : base(switchData) { }
    }
}
