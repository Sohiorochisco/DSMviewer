using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    /// <summary>
    /// Represents a three phase distribution transformer
    /// </summary>
    public class PowerTransformer : Equipment
    {
        public PowerTransformer(DataRow data):base(data)
        {
            equipmentType = EquipmentTopoTypes.None;
            //Just use a devault value for now
            BaseKVA = (float)500;
            Ends = new List<TransformerEnd>();
        }

        public IList<TransformerEnd> Ends { get; private set; }

        /// <summary>
        /// Rated power for the transformer
        /// </summary>
        public float BaseKVA{get; private set;}

        public double TurnsRatio()
        {
            return Ends[0].BaseKV / Ends[1].BaseKV;
        }

        public IEnumerable<Terminal> Terminals()
        {
            var terms = new HashSet<Terminal>();
            foreach (PowerTransformer.TransformerEnd end in Ends)
            {
                terms.Add(end.Terminal1);
            }
            return terms;
        }


        /// <summary>
        /// Returns the percentage reactance between the two coils
        /// </summary>
        /// <param name="highEnd"></param>
        /// <param name="lowEnd"></param>
        /// <returns></returns>
        public float Xpercentage(PowerTransformer.TransformerEnd highEnd, PowerTransformer.TransformerEnd lowEnd)
        {
            if (highEnd.BaseKV < lowEnd.BaseKV)
            {throw new ArgumentOutOfRangeException("lowEnd","lowEnd should have a lower BaseKV than highEnd");}
            return (float)
                ((BaseKVA * (highEnd.X + lowEnd.X)) / (highEnd.BaseKV - lowEnd.BaseKV));
        }

        
        

        /// <summary>
        /// A conducting end (temrminal and winding) of a powertransformer
        /// </summary>
        public class TransformerEnd : ConductingEquipment
        {
            /// <summary>
            /// The PowerTransformer of which this end is a part
            /// </summary>
            private PowerTransformer parentTransformer;
            public PowerTransformer ParentTransformer{get{return parentTransformer;}}

            
            //The shunt and series impedence and admittance for the transformer model
            private float r;
            private float b;
            private float x;
            private float g;
            private float r0;
            private float b0;
            private float g0;
            private float x0;
            /// <summary>
            /// positive sequence series resistance
            /// </summary>
            public float R  { get { return r; } }
            /// <summary>
            /// positive sequence shunt susceptance
            /// </summary>
            public float B  { get { return b; } }
            /// <summary>
            /// positive sequence shunt conductance
            /// </summary>
            public float G  { get { return g; } }
            /// <summary>
            /// positive sequence series reactance
            /// </summary>
            public float X  { get { return x; } }
            /// <summary>
            /// zero-sequence series resistance
            /// </summary>
            public float R0 { get { return r0; } }
            /// <summary>
            /// zero-sequence shunt susceptance
            /// </summary>
            public float B0 { get { return b0; } }
            /// <summary>
            /// zero-sequence series reactance
            /// </summary>
            public float X0 { get { return x0; } }
            /// <summary>
            /// zero-sequence shunt conductance
            /// </summary>
            public float G0 { get { return g0; } }

            public float BaseKV { get; private set; }


            public float Rpercentage()
            {
                return (float)((parentTransformer.BaseKVA / BaseKV) * r);
            }

            public PhaseShuntConnectionKind ConnectionType { get; private set; }

            
            public TransformerEnd(DataRow data, PowerTransformer parent)
                : base(data)
            {
                parentTransformer = parent;
                BaseKV = (float)(double)data["BaseKV"];
                r = (float)(double)data["R"];
                
                x = (float)(double)data["X"];
                g = (float)(double)data["G"];
                r0 = (float)(double)data["R0"];
                
                g0 = (float)(double)data["G0"];
                x0 = (float)(double)data["X0"];
                ConnectionType = (PhaseShuntConnectionKind)Enum.Parse
                (
                typeof(PhaseShuntConnectionKind),
                (string)data["phaseConnection"],
                true
                );
                
                equipmentType = EquipmentTopoTypes.Shunt;

            }

        }
    }
}
