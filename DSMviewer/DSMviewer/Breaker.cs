using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class Breaker : ProtectedSwitch
    {
       
        
        public Breaker(DataRow switchData) : base(switchData) { }
        
    }
}
