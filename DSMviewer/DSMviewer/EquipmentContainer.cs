using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using ModelManipulation;

namespace IdentifiedObjects
{
    public abstract class EquipmentContainer : ConnectivityNodeContainer,
        IEnumerable<Equipment> 
    {
        protected SubGeographicalRegion serviceArea;
        /// <summary>
        /// The Area serviced by the distribution system of which this is a part
        /// </summary>
        public SubGeographicalRegion ServiceArea { get { return serviceArea; } }


        protected Dictionary<String, Equipment> equipments;
        //Questionable implementation of IEnumerable to permit use of foreach; a better option probably exists
        public IEnumerator<Equipment> GetEnumerator()
        {
            //creates an entirely new IEnumerable just for the enumerator...
            return equipments.Values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return equipments.Cast<Equipment>().GetEnumerator(); 
        }

        public virtual Equipment this[string name] 
        {
            get { return equipments[name]; }
            set { equipments[name] = value; }
        }
        public override int GetHashCode()
        {
            return name.GetHashCode() ^ type.GetHashCode() ^ serviceArea.Name.GetHashCode(); 
        }

       
        public int EquipmentCount()
        { return equipments.Count; }

        //equipments will not be initialized unless by a method in a child container class.
        public EquipmentContainer(string newName, SubGeographicalRegion thisArea) : base(newName) 
        {
            equipments = new Dictionary<string, Equipment>();
            serviceArea = thisArea;
        }

    }
}
