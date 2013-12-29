using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Numerics;

namespace IdentifiedObjects
{
    class Switch : ConductingEquipment 
    {
        public bool NormallyOpen { get; set; }
        public Terminal Terminal2 {get; set; }
        public string Term2Name { get; set; }
        

        public SwitchState SwitchState { get; set; }


        
        public Switch(DataRow switchData)
            : base(switchData)
        {
            equipmentType = EquipmentTopoTypes.Switch;
            NormallyOpen    = (bool)switchData["NormallyOpen"];
            Term2Name       = (string)switchData["Terminal2"];
            if (NormallyOpen)
            {
                SwitchState = SwitchState.open;
            }
            else
            {
                SwitchState = SwitchState.closed;
            }
        }
    }
}
