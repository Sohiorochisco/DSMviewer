using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Numerics;

namespace IdentifiedObjects
{
    /// <summary>
    /// EnergyConsumer represents a load. Since currently the parameters are the same for all phases, for unbalanced loads use multiple EnergyConsumer Instances
    /// </summary>
    public class EnergyConsumer : ConductingEquipment
    {
        protected float pfixed;
        protected float qfixed;
        public float Pfixed { get { return pfixed; } protected set { pfixed = value; } }
        public float Qfixed { get { return qfixed; } protected set { qfixed = value; } }
        internal float Pmult { set; private get; }
        internal float Qmult { set; private get; }
        /// <summary>
        /// Returns the nominal Real power for the current time 
        /// </summary>
        public float Pnominal
        {
            get { return (float)(Pmult * pfixed); }
        }
        /// <summary>
        /// Returns the nominal reactive power for the current time
        /// </summary>
        public float Qnominal
        {
            get { return (float)(Qmult * qfixed); }
        }
        public Complex SNominal()
        {
            return new Complex(Pnominal, Qnominal);
        }

        public Complex[] Currents(Complex[] voltages,float baseKV)
        {
            var currents = new Complex[3];
            if (Terminal1.Phases.HasFlag(PhaseCode.A))
            { currents[0] = current(voltages[0], baseKV); }
            if (Terminal1.Phases.HasFlag(PhaseCode.B))
            { currents[1] = current(voltages[1], baseKV); }
            if (Terminal1.Phases.HasFlag(PhaseCode.C))
            { currents[2] = current(voltages[2], baseKV); }

            return currents;
            
        }
        private Complex current(Complex voltage, float baseKV)
        {
            if (voltage.Real < 0.5 * baseKV)
            {
                return 0;
            }
            var obsPower = SNominal();
            //Assuming ZIP models for now

            var ConsS = LoadResponse.PConstantPower * obsPower;
            var ConsZ = LoadResponse.PConstantImpedance * obsPower;
            var ConsI = LoadResponse.PConstantCurrent * obsPower;

            var IconsS = Complex.Conjugate(Complex.Divide(ConsS, voltage));
            Complex IconsZ;
            if (ConsZ != Complex.Zero)
            {
                IconsZ = ((baseKV * baseKV) / Complex.Conjugate(ConsZ)) * voltage;
            }
            else 
            {
                IconsZ = Complex.Zero;
            }
            var IconsI = Complex.Conjugate(ConsI / baseKV);

            return IconsI + IconsS + IconsZ;             
        }
        public DateTime LastUpdateTime { get; internal set; }

        /// <summary>
        /// used to create a new instance of LoadResponseCharacteristic
        /// </summary>
        /// <param name="data"></param>
        public static void AddLoadModel(DataRow data)
        {
            loadModels.Add((string)data["Name"], new LoadResponseCharacteristic(data));
        }
        private string loadResponseName;
        private static  Dictionary<string,LoadResponseCharacteristic> loadModels;
        private PhaseShuntConnectionKind phaseConnection;
        public PhaseShuntConnectionKind PhaseConnection { get { return phaseConnection; } }

        /// <summary>
        /// returns a load model from a static collection of loadresponse objects, give the loadResponseName
        /// </summary>
        public LoadResponseCharacteristic LoadResponse { get { return loadModels[loadResponseName]; } }

        static EnergyConsumer()
        {
            loadModels = new Dictionary<string, LoadResponseCharacteristic>();
        }
      
        public EnergyConsumer(DataRow objFields) : base(objFields) 
        {
            
            qfixed = (float)(double)objFields["qFixed"];
            pfixed = (float)(double)objFields["pFixed"];
            loadResponseName = (string)objFields["LoadResponse"];
            equipmentType = EquipmentTopoTypes.Shunt;
            phaseConnection = (PhaseShuntConnectionKind)Enum.Parse
                (
                typeof(PhaseShuntConnectionKind),
                (string)objFields["phaseConnection"], 
                true
                );
            
        }

    }
}
