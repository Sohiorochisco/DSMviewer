using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 
using System.Data;

namespace IdentifiedObjects
{
    /// <summary>
    /// Currently does not expand on IdentifiedObject, we have simply to be more CIM compliant
    /// </summary>
    public abstract class PowerSystemResource : IdentifiedObject
    {
        /// <summary>
        /// Returns a string which lists all of the field names and values for the GUI.
        /// </summary>
        /// <returns></returns>
        public string GetFieldSummary()
        {
            //obtain type using reflection
            var thisType = this.GetType();

            //returns a fieldinfo array for the given type
            var fields = thisType.GetFields();

            var fieldSummary = new StringBuilder();
            for (int i = 0; i < fields.Length; i++)
            {
                //Should probably be placed in a try block, and check some attribute rather than displaying everything
                fieldSummary.AppendFormat("{0} = {1}",fields[i].GetValue(this));
                fieldSummary.AppendLine();
                
            }

            return fieldSummary.ToString();
        }


        public PowerSystemResource(DataRow data) : base(data)
        {
            
        }
    }
}
