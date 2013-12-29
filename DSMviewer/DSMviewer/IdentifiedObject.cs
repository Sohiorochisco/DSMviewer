using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    /// <summary>
    /// Top level class for all objects in the model
    /// </summary>
   abstract public class IdentifiedObject
    {
       
       protected string name;
       public string Name
       {
           get
           {
               return name;
           }
       }


       /// <summary>
       /// Gives the name of the type, to use instead of reflection
       /// </summary>
       public string Type { get {return type; } }
       protected string type;

       /// <summary>
       /// Most IdentifiedObjects are constructed from a DataRow, pulled from database
       /// </summary>
       /// <param name="objValues"></param>
       public IdentifiedObject(DataRow objValues)
       {
           name = objValues["Name"].ToString();
           type = objValues["Type"].ToString();           
       }

       public IdentifiedObject() { }

    }
}
