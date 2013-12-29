using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ModelManipulation
{
    /// <summary>
    /// The standard interface for obtaining model data
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// Returns a datatable, where each row contains a definition of an IdentifiedObject
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        DataTable GetObjectDefinitions(string typeName);

        /// <summary>
        /// Overload to allow filters based on a field value
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        DataTable GetObjectDefinitions(string typeName, string field, string value );

         // Get the load data for a particular time. The TimestepLoadDelegate
        //  will be called once for each load in the model, providing the name
        //  of that load and its most recent power consumption and power factor
        // returns the number of times the delegate was called.
        int GetTimestepLoadData(DateTime t, TimestepLoadDelegate c);
    }
    //A TimestepLoadDelegate will be implemented by the caller of GetTimestepLoadData
    // Calls provide the load power for a given EnergyConsumer referenced by name
    // because the DataSource does not know which model object that name refers to
    //This TimestepLoadDelegate will presumably update the appropriate object
    //Enumeration of the loads will stop if false is returned.
    public delegate bool TimestepLoadDelegate(DateTime t, string name, float Pfixed, float Qfixed);
}

