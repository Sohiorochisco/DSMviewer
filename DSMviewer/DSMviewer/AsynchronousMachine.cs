using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class AsynchronousMachine : RotatingMachine
    {
        public AsynchronousMachine(DataRow data) : base(data) { }
    }
}
