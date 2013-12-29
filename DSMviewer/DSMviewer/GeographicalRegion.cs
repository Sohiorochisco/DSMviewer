using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModelManipulation;
using System.Data;

namespace IdentifiedObjects
{
    
    /// <summary>
    /// Represents the area in which the distribution system is located
    /// </summary>
    public class SubGeographicalRegion : IdentifiedObject
    {
        private AreaBuilder builder;

        public SubGeographicalRegion(string thisName)
        {
            name = thisName;
            subStations = new Dictionary<string, EquipmentContainer>();
            positions = new Dictionary<string, PositionPoint>();
            locations = new Dictionary<string, Location>();
            builder = new AreaBuilder(this);
            builder.BuildModel();
        }
        //private backing fields
        private Dictionary<string,EquipmentContainer> subStations;
        private Dictionary<string,PositionPoint> positions;
        private Dictionary<string, Location> locations;

        /// <summary>
        /// All of the Lines and Substations located within the system
        /// </summary>
        public IDictionary<string, EquipmentContainer> SubStations 
        { 
            get { return subStations; }
        }

        /// <summary>
        /// All of the defined Positions within the area
        /// </summary>
        public IDictionary<string,PositionPoint> Positions
        { 
            get { return positions; } 
        }

        /// <summary>
        /// Returns all EquipmentContainers currently located within the SubGeographicalArea
        /// </summary>
        /// <returns></returns>
        public IDictionary<string,EquipmentContainer> GetEquipmentContainers()
        {
            //messy, brute-force, inefficient way of doing things...
            IDictionary<string, EquipmentContainer> allContainers = new Dictionary<string, EquipmentContainer>();              
            foreach (Substation ss in subStations.Values)
            {
                allContainers.Add(ss.Name,ss);
                foreach (KeyValuePair<string, Line> line in ss.Lines)
                {
                    allContainers.Add(line.Key,line.Value);
                }
            }
            return allContainers;

        }
        

        /// <summary>
        /// All of the defined Locations within the area
        /// </summary>
        public IDictionary<string,Location> Locations
        {
            get { return locations; }
        }


        protected class AreaBuilder : ModelBuilder
        {
            private SubGeographicalRegion area;

            public AreaBuilder(SubGeographicalRegion thisArea)
                : base(thisArea)
            {
                area = thisArea;
                containedTypeNames = new string[] { "Location", "PositionPoint", "Substation" };
            }

            protected override void constructAndConnect(Dictionary<string, DataTable> typesToBuild)
            {
                makeLocationsAndPoints(typesToBuild["PositionPoint"], typesToBuild["Location"]);
                makeSubStations(typesToBuild["Substation"]);
            }

            
            protected void makeLocationsAndPoints(DataTable pointVals, DataTable locationVals)
            {
                foreach (DataRow row in pointVals.Rows)
                {
                    area.positions.Add(row["Name"].ToString(), new PositionPoint(row));
                }
                foreach (DataRow row in locationVals.Rows)
                {
                    area.locations.Add(row["Name"].ToString(), new Location(row, area.Positions[row["PositionPoint"].ToString()]));
                }
            }
            protected void makeSubStations(DataTable data)
            {
                foreach (DataRow row in data.Rows)
                {
                    area.subStations.Add((string)row["Name"],new Substation(area,(string)row["Name"]));
                }
            }
            
        }

    }

}
