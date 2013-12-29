using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class LoadBreakSwitch : ProtectedSwitch
    {
        public LoadBreakSwitch(DataRow row) : base(row) { }
    }
}
