using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModelManipulation;
using System.Data;
using System.Xml;

namespace IdentifiedObjects
{
    /// <summary>
    /// A Substation is a collection of lines, as well as a container of all equipment located at the substation site
    /// </summary>
    class Substation : EquipmentContainer
    {
        public Substation(SubGeographicalRegion thisRegion, string thisName)
            : base(thisName, thisRegion)
        {
            this.builder = new SubstationBuilder(this);
            this.lines = new Dictionary<string, Line>();
            builder.BuildModel();
        }

        private SubstationBuilder builder;
        private Dictionary<string, Line> lines;
        public Dictionary<string, Line> Lines
        {
            get { return lines; }
            set { lines = value; }
        }

        
        private class SubstationBuilder : ModelBuilder
        {
            private Substation substation;
            public SubstationBuilder(Substation thisSubstation)
                :base(thisSubstation)
            {
                substation = thisSubstation; 
                containedTypeNames = new string[]{"Line"};
            }

            /// <summary>
            /// Called from within the BuildModel public method
            /// </summary>
            /// <param name="typesToBuild"></param>
            protected override void  constructAndConnect(Dictionary<string,DataTable> typesToBuild)
            {
                
                foreach (DataRow row in typesToBuild["Line"].Rows) 
                {
                    var lineName = (string)row["Name"];
                    var sourceBus = (string)row["Bus1"];
                    var baseKV = (float)(double)row["BaseKV"];
                    substation.lines.Add(lineName,new Line(lineName,substation.serviceArea,sourceBus,baseKV));
                }
            }
        }
    }
}
