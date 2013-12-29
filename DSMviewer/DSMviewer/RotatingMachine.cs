using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class RotatingMachine :RegulatingCondEq
    {
        public RotatingMachine(DataRow data) : base(data) { }
    }
}
