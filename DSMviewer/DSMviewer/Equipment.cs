using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    /// <summary>
    /// Parent class of all physical hardware in the system
    /// </summary>
    public abstract class Equipment : PowerSystemResource
    {
        public Line Feeder { get; protected set; }
        //Indicates whether given type or instance contains other equipment, usually false
        public bool Aggregate { get; protected set; }

        protected EquipmentTopoTypes equipmentType;
        /// <summary>
        /// Used only by ConductingEquipment, but kept at Equipment level in order to select from EquipmentContainers. For non-conducting equipment set to "None"
        /// </summary>
        public EquipmentTopoTypes EquipmentType
        {
            get
            {
                return equipmentType;
            }
        }

        public Equipment(DataRow objFields) : base(objFields)
        { }
    }
}
