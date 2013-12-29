using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    public class Terminal : IdentifiedObject, IEqualityComparer<Terminal>
    {
        public ConductingEquipment ParentEquipment{get; private set;}
        public ConnectivityNode ConnectionPoint{get; internal set;}
        public String ConnectionPointName { get; private set; }
        public PhaseCode Phases { get; private set; }
        public bool Connected { get; internal set; }
        public Location Location { get; private set; }


        /// <summary>
        /// Specifies that to be considered equal, two terminal references must be to the same instance
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(Terminal x, Terminal y)
        {
            return Object.ReferenceEquals(x, y);
        }
        /// <summary>
        /// Included in order to implement IEqualityComparer
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(Terminal term)
        {
            return term.name.GetHashCode() ^ term.ParentEquipment.GetHashCode();
        }
        
                

        public Terminal(DataRow objValues, ConductingEquipment newParentEquipment, SubGeographicalRegion area) : base(objValues)
        {
            ConnectionPointName = (string)objValues["ConnectionPoint"];
            ParentEquipment = newParentEquipment;
            Phases = (PhaseCode)Convert.ToInt32(objValues["Phases"]);
            this.Location = area.Locations[(string)objValues["Location"]];
                       
        }
    }
}
