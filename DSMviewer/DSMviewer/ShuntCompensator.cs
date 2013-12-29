using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
   public class ShuntCompensator : RegulatingCondEq
    {
       /// <summary>
       /// Conductance data for the shunt
       /// </summary>
       private float b;
       private float b0;
       private float c;
       private float c0;
       public float B { get { return b; } }
       public float B0 { get { return b0; } }
       public float C { get { return c; } }
       public float C0 { get { return C; } }

       /// <summary>
       /// Gives whether the shunt is connected in a Star or Delta configuration
       /// </summary>
       private PhaseShuntConnectionKind phaseConnection;
       public PhaseShuntConnectionKind PhaseConnection;
       private bool grounded;
       public bool Grounded { get { return grounded; } }
        
        
        
        

        public ShuntCompensator(DataRow data) : base(data)
        {
            b   = (float)data["b"];
            b0  = (float)data["b0"];
            c   = (float)data["c"];
            c0  = (float)data["c0"];
            grounded = (bool)data["Grounded"];
            //I think this should work like converting from an int - I will check to make sure
            phaseConnection = (PhaseShuntConnectionKind)data["phaseConnection"];

        }

    }
}
