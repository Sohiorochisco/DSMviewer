using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Numerics;

namespace IdentifiedObjects
{
    public class ACLineSegment : Conductor
    {
        /// <summary>
        /// lists all defined models for ACLineSegments
        /// </summary>
        internal static Dictionary<string, LineModel> LineModels { get; private set; }
        static ACLineSegment(){LineModels = new Dictionary<string, LineModel>();}

        protected string lineModelname;
        public String LineModelname { set { lineModelname = value; } }
        public LineModel LineModel{ get{ return LineModels[lineModelname];}}
        private Complex[][] zmatrix;

        /// <summary>
        /// Gets impedance per length from LineModel, gets actual line impedances by multiplying by length
        /// </summary>
        /// <returns></returns>
        public Complex[][] PhaseImpedance()
        {
            if (zmatrix == null)
            {
                zmatrix = this.LineModel.PhaseImpedancePerLength();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        zmatrix[i][j] = zmatrix[i][j] * (Length / 5280);                        
                    }
                    
                }
            }
            
            return zmatrix;
            
        }
        public ACLineSegment(DataRow lineData)
            : base(lineData)
        {
            lineModelname = (string)lineData["LineModel"];
            equipmentType = EquipmentTopoTypes.Conductor;
        }

    }
}
