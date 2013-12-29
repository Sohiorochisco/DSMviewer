using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Numerics;

namespace IdentifiedObjects
{
    /// <summary>
    /// Parent class for all equipment galvanically connected to the distribution system
    /// </summary>
    public abstract class ConductingEquipment : Equipment
    {
        /// <summary>
        /// Every ConductingEquipment type instance contains at least one Terminal
        /// </summary>
        public Terminal Terminal1 { get; set; }

        /// <summary>
        /// Used during model building
        /// </summary>
        internal String Term1Name { get; set; }

        

        /// <summary>
        /// Used to store constructors for ConductingEquipment types
        /// </summary>
        static private Dictionary<String,Func<DataRow,ConductingEquipment>> constructors;

        /// <summary>
        /// Used to construct ConductingEquipment types
        /// </summary>
        /// <param name="condData"></param>
        /// <returns></returns>
        static public ConductingEquipment newConductingEquipment(DataRow condData) 
        {
            var gearType = (string)condData["Type"];
            return constructors[gearType](condData);
        }

        /// <summary>
        /// builds the constructors dictionary
        /// </summary>
        static ConductingEquipment()
        {
            constructors = new Dictionary<String,Func<DataRow,ConductingEquipment>>();
            constructors["ACLineSegment"] = x => new ACLineSegment(x);
            constructors["Disconnector"] = x => new Disconnector(x);
            constructors["Breaker"] = x => new Breaker(x);
            constructors["Fuse"] = x => new Fuse(x);
            constructors["Jumper"] = x => new Jumper(x);
            constructors["Recloser"] = x => new Recloser(x);
            constructors["Switch"] = x => new Switch(x);
            constructors["EnergyConsumer"] = x => new EnergyConsumer(x);
        }
        

        
        public ConductingEquipment(DataRow objFields) : base(objFields)
        {
            Term1Name = (String)objFields["Terminal1"];
        }
    }
}
