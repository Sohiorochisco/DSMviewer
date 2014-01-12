using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    /// <summary>
    /// Parent class for transmission line types
    /// </summary>
    public abstract class Conductor : ConductingEquipment
    {
        /// <summary>
        /// Currently, no state for conductors. Will probably be replaced with a state indicating
        /// whether the line is overloaded
        /// </summary>
        /// <returns></returns>
        internal override string State()
        {
            return "";
        } 
        public string Term2Name{get;set;}
        /// <summary>
        /// LoopMember indicates whether the conductor is part of a loop in the system topology.
        /// </summary>
        public bool LoopMember { get; set; }

        public float Length { get; set; }

        /// <summary>
        /// All Conductors must have at least two terminals
        /// </summary>
        public Terminal Terminal2 { get; set; }

        protected ConnectivityNode getOtherNeighbor(Terminal term)
        {
            Conductor parentConductor = term.ParentEquipment as Conductor;
            if (ReferenceEquals(term, parentConductor.Terminal1))
            {
                //return the connected node that does not contain term.
                return parentConductor.Terminal2.ConnectionPoint;
            }
            if (ReferenceEquals(term, parentConductor.Terminal2))
            {
                return parentConductor.Terminal1.ConnectionPoint;
            }
            return null;
        }


        
        public Conductor(DataRow objFields) : base(objFields) 
        {
            Term2Name = (string)objFields["Terminal2"];
            Length =    (float)(double)objFields["Length"];

        }
        
    }
}
