using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    class Cut : Switch
    {
        public Cut(DataRow row) : base(row) { }
    }
}
