﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class ConformLoad : EnergyConsumer
    {
        public ConformLoad(DataRow row) : base(row) { }
    }
}
