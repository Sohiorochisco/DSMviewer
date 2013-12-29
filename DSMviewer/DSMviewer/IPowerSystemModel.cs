using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IdentifiedObjects;


namespace ModelManipulation
{
    /// <summary>
    /// Interface for all interactions between the GUI and the model
    /// </summary>
    interface IPowerSystemModel
    {
        /// <summary>
        /// Builds the model, returns a list of all of the EquipmentContainers in the model
        /// </summary>
        /// <returns></returns>
        Dictionary<string,EquipmentContainer> BuildModel();

        

        /// <summary>
        /// Loads a particular line into the powerflow solver. Eventually if a substation is given, will load all lines from a given source bus
        /// </summary>
        /// <returns></returns>
        bool RunTimeStep(Line thisLine, DateTime t);

        /// <summary>
        /// Gets the system service area
        /// </summary>
        /// <returns></returns>
        SubGeographicalRegion GetRegion();
       
    }

    
}







