using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class NonConformLoad : EnergyConsumer
    {
        public NonConformLoad(DataRow row) : base(row) { }
    }
}
