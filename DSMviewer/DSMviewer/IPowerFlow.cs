using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenDSSengine;
using IdentifiedObjects;

namespace IdentifiedObjects
{
    public interface IPowerFlow
    {
        /// <summary>
        /// Provides the powerflow program with adequate information to construct the Ybus matrix for the power system
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="conductors"></param>
        /// <param name="sourceNode"></param>
        /// <param name="?"></param>
        void NewACFeederCircuit
            (
                Line newFeeder
                
            );

        /// <summary>
        /// Indicates the current Line loaded into the power flow solution software
        /// </summary>
        /// <returns></returns>
        Line ReturnActiveFeeder();


        /// <summary>
        /// RunPowerFlow updates the voltage fields for each ConnectivityNode in the current Line object; returns true if successful 
        /// </summary>
        /// <param name="currentLoads"></param>
        bool RunPowerFlow(IEnumerable<EnergyConsumer> currentLoads);
    }
}
