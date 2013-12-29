using System;using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    /// <summary>
    /// Used to model a voltage regulator
    /// </summary>
    class StaticVarCompensator : RegulatingCondEq
    {
        public StaticVarCompensator(DataRow data) : base(data) { }
    }
}
