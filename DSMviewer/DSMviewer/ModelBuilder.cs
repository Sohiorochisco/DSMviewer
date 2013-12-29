using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IdentifiedObjects;
using System.Data;

namespace ModelManipulation
{//Parent Class for all classes used to instantiate and connect equipment (building the model)
//Likely complete, but has not yet been compiled as part of a working program in current state
    public abstract class  ModelBuilder 
    {
        protected IdentifiedObject modelObject;
        protected static IDataSource datasource;
        protected string[] containedTypeNames;
        protected string   filterField;



        public virtual void BuildModel()
        {
            var typesToBuild = new Dictionary<string, DataTable>();
            //pull in all data necessary for instantiation
            
            foreach (string typeName in containedTypeNames)
            {
                if (filterField != null)
                {
                    typesToBuild.Add(typeName, datasource.GetObjectDefinitions(typeName, filterField, modelObject.Name));
                }
                else
                {
                    typesToBuild.Add(typeName, datasource.GetObjectDefinitions(typeName));
                }
            }
            constructAndConnect(typesToBuild);
        }
            
            

            
                        
        
        /// <summary>
        /// Use this class to perform all instantiation and pass references between contained types
        /// </summary>
        /// <param name="typesToBuild"></param>
        abstract protected void constructAndConnect(Dictionary<string, DataTable> typesToBuild);

   

        public  ModelBuilder(IdentifiedObject thisModelObject)
        {
            modelObject = thisModelObject;
        }
            
       


    }
}
