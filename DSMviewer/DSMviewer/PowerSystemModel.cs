using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModelManipulation;
using IdentifiedObjects;
using System.Data;
using System.IO;

namespace IdentifiedObjects
{
    /// <summary>
    /// Implements IPowerSystemModel to handle interaction between the GUI and the model
    /// </summary>
    public class PowerSystemModel : IdentifiedObject , IPowerSystemModel
    {
        private IPowerFlow powerflow;
        private IDataSource datasource;
        public PowerSystemModel(IDataSource thisDataSource)
        {
            datasource = thisDataSource;
        }

        # region IPowerSystemModel implementation

        /// <summary>
        /// Builds the model
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,EquipmentContainer> BuildModel()
        {
            builder = new TopLevelBuilder(this, this.datasource);
            builder.BuildModel();
            //Get the current working directory for the application
            string dir = Directory.GetCurrentDirectory();
            //Create a directory for the OpenDSS script files produced by the powerflow class
            string newDir = dir + "//OpenDSS_Scripts";
            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }
            //Create a new OpenDSSPowerFlow, and pass the file path for the scripts directory
            powerflow = new LadderPowerFlow();
            var containerList = new Dictionary<string,EquipmentContainer>();
            foreach (Substation station in thisRegion.SubStations.Values)
            {
                foreach (Line line in station.Lines.Values)
                {
                    containerList.Add(line.Name,line);
                    
                }
                containerList.Add(station.Name,station);
            }
            return containerList;
        }

        /// <summary>
        /// Loads a particular line into the powerflow solver. If a substation is given, will load all lines from a given source bus
        /// </summary>
        /// <returns></returns>
        public bool RunTimeStep(Line thisLine, DateTime t)
        {
            thisLine.UpdateLoads(t);
            if (!Equals(thisLine, powerflow.ReturnActiveFeeder()) || thisLine.Revised)
            {
                powerflow.NewACFeederCircuit(thisLine);
            }
            
            //Filter to get all energyConsumers
            var loads = thisLine.Where(x => x.Type == "EnergyConsumer");
            //Cast all to energyConsumers, run powerflow
            return powerflow.RunPowerFlow(loads.Cast<EnergyConsumer>());
        }

        public SubGeographicalRegion GetRegion()
        {
            return thisRegion;
        }

       
        
        #endregion 

        # region Private Members


        private SubGeographicalRegion thisRegion;

        private TopLevelBuilder builder;

        private class TopLevelBuilder : ModelBuilder
        {
            private PowerSystemModel topLevel;
            public TopLevelBuilder(PowerSystemModel thisTopLevel, IDataSource thisDataSource): base(thisTopLevel)
            {
                topLevel = thisTopLevel;
                //datasource set at the top level, and then used by every other ModelBuilder instance
                datasource = thisDataSource;
                containedTypeNames = new String[] { "LineModel","LoadResponseCharacteristic" };
            }
            protected override void constructAndConnect(Dictionary<string,DataTable> typesToBuild)
            {
                ACLineSegment.LineModels.Clear();
                EnergyConsumer.ClearLoadModels();

                //Add all lineModels to the ACLineSegment type
                foreach (DataRow row in typesToBuild["LineModel"].Rows)
                {
                     ACLineSegment.LineModels.Add((string)row["Name"], new LineModel(row));
                }

                //Add all LoadResponseCharacteristics to the EnergyConsumer type
                foreach (DataRow row in typesToBuild["LoadResponseCharacteristic"].Rows)
                {
                    EnergyConsumer.AddLoadModel(row);
                }
                topLevel.thisRegion = new SubGeographicalRegion("Area");
            } 

        }




        #endregion
    }
}
