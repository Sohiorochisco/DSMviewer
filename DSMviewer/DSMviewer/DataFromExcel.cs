using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IdentifiedObjects;
using System.Data;
using System.IO;
using System.Data.OleDb;

namespace ModelManipulation
{
    /// <summary>
    /// Class to test pulling data into the model. Requires either Office or a stand-alone install of ACE to work
    /// </summary>
    class DataFromExcel : IDataSource
    {
        private DataTable profile;
        private string connectionString(string filename)
        {
            return String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES\";", filename);
        }

        /// <summary>
        /// reads out the contents of a single Excel worksheet to a DataTable
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        private DataTable readXslx(string filename, string sheetName)
        {
            DataTable tableOut;
            OleDbDataAdapter dataAdaptor;
            using (var connection = new OleDbConnection(connectionString(filename)))
            {
                tableOut = new DataTable();
                connection.Open();
                dataAdaptor = new OleDbDataAdapter("SELECT * FROM [" + sheetName + "$]", connection);
                dataAdaptor.Fill(tableOut);
                return tableOut;
            }
        }



        /// <summary>
        /// The Excel spreadsheet is assumed to have a seperate sheet for each type.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public DataTable GetObjectDefinitions(string typeName)
        {
            return readXslx(fileName, typeName);
        }

        public DataTable GetObjectDefinitions(string typeName, string field, string value)
        {
            //Perform a LINQ query on the table to filter based on containers.
            var rowEnumerable = from row in GetObjectDefinitions(typeName).AsEnumerable()
                               where row.Field<string>(field) == value
                               select row;
            if (rowEnumerable.Count() != 0)
                return rowEnumerable.CopyToDataTable();
            else
                return null;

        }

        /// <summary>
        /// Implementation for single feeder, single timestep (essentially it does nothing but replace values with themselves for testing)
        /// </summary>
        /// <param name="t"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public int GetTimestepLoadData(DateTime t, TimestepLoadDelegate c)
        {
            //Currently only gives the proper load for the time of day
            int daytime = (int)(t.TimeOfDay.TotalHours * 2);
            int countLoadUpdates = 0;
            var loads = GetObjectDefinitions("EnergyConsumer");
            foreach (DataRow row in loads.Rows)
            {
                var n = (string)row["Name"];
                var selectString = String.Format("Time = {0}", daytime.ToString());
                var loadsAtTime = profile.Select(selectString);
                var p = (float)(double)loadsAtTime[0]["P"];
                var q = (float)(double)loadsAtTime[0]["Q"];
                if (c(t, n, p, q))
                {
                    countLoadUpdates++;
                }
                else
                {
                    return 0;
                }
            }
            return countLoadUpdates;
        }



        string fileName;


        public DataFromExcel(string file)
        {
            fileName = file;
            profile = this.GetObjectDefinitions("LoadProfile");
            
        }

    }
}
