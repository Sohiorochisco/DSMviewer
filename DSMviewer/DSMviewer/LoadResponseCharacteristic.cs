using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    public class LoadResponseCharacteristic : IdentifiedObject
    {
        private float pConstantCurrent;
        /// <summary>
        /// gives ratio of total real power modelled with constant current
        /// </summary>
        public float PConstantCurrent{get{return pConstantCurrent;}}
        private float pConstantImpedance;
        /// <summary>
        /// gives ratio of total real power modelled with constant impedance
        /// </summary>
        public float PConstantImpedance{get{return pConstantImpedance;}}
        private float pConstantPower;
        /// <summary>
        /// gives ratio of total real power modelled as constant
        /// </summary>
        public float PConstantPower{get{return pConstantPower;}}
        private float pFrequencyExponent;
        /// <summary>
        /// gives ratio of total real power modelled with an exponential dependence on frequency
        /// </summary>
        public float PFrequencyExponent{get{return pFrequencyExponent;}}
        private float pVoltageExponent;
        /// <summary>
        /// gives ratio of total real power modelled with an exponential depence on voltage
        /// </summary>
        public float PVoltageExponent{get{return pVoltageExponent;}}
        private float qConstantCurrent;
        /// <summary>
        /// gives ratio of total reactive power modelled with constant current
        /// </summary>
        public float QConstantCurrent{get{return qConstantCurrent;}}
        private float qConstantImpedance;
        /// <summary>
        /// gives ratio of total reactive power modelled with constant impedence
        /// </summary>
        public float QConstantImpedance{get{return qConstantImpedance;}}
        private float qConstantPower;
        /// <summary>
        /// gives ratio of total reactive power modelled as constant
        /// </summary>
        public float QConstantPower{get{return qConstantPower;}}
        private float qFrequencyExponent;
        /// <summary>
        /// gives ratio of total reactive power modelled with an exponential dependence on frequency
        /// </summary>
        public float QFrequencyExponent{get{return qFrequencyExponent;}}
        private float qVoltageExponent;
        /// <summary>
        /// gives ratio of total reactive power modelled with an exponential dependence on voltage
        /// </summary>
        public float QVoltageExponent{get{return qVoltageExponent;}}

        /// <summary>
        /// indicates whether the current load model is exponential
        /// </summary>
        private bool exponentModel;

        public LoadResponseCharacteristic(DataRow data)
            : base(data)
        {
            exponentModel = (bool)data["exponentModel"];
            if(exponentModel)
            { 
                pFrequencyExponent = (float)(double)data["pFrequencyExponent"];
                pVoltageExponent = (float)(double)data["pVoltageExponent"];
                qFrequencyExponent = (float)(double)data["qFrequencyExponent"];
                qVoltageExponent = (float)(double)data["qVoltageExponent"];
                return;
            }

            pConstantCurrent = (float)(double)data["pConstantCurrent"];
            pConstantImpedance = (float)(double)data["pConstantImpedance"];
            pConstantPower = (float)(double)data["pConstantPower"];            
            qConstantCurrent = (float)(double)data["qConstantCurrent"];
            qConstantImpedance = (float)(double)data["qConstantImpedance"];
            qConstantPower = (float)(double)data["qConstantPower"];
            
        }
        


    }
}
