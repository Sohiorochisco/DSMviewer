using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class SynchronousMachine : RotatingMachine
    {
        public SynchronousMachine(DataRow data) : base(data) { }
    }
}
