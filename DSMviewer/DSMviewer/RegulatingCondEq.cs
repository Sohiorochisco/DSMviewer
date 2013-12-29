using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    public class RegulatingCondEq : ConductingEquipment
    {
        private PhaseCode phases;
        public PhaseCode Phases { get { return phases; } }
        
        public RegulatingCondEq(DataRow regData) : base(regData) 
        {
            phases = (PhaseCode)regData["Phases"];
        }
    }
}
