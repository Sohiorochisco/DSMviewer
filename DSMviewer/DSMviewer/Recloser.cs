﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class Recloser : ProtectedSwitch
    {
        
        public Recloser(DataRow switchData) : base(switchData) 
        { }
    }
}
