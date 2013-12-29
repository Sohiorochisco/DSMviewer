using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    /// <summary>
    /// Controller which changes the tap position on a transformer winding. 
    /// </summary>
    class TapChanger : PowerSystemResource
    {
        public TapChanger(DataRow row) : base(row) { }
    }
}
