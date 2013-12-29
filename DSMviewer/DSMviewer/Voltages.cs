using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IdentifiedObjects
{
    /// <summary>
    /// The Voltage angles and magnitudes for all phases 
    /// </summary>
    public class Voltages : IdentifiedObject
    {
        
        /// <summary>
        /// Creates new Voltages object with all of the phases specified
        /// </summary>
        /// <param name="thesePhases"></param>
        public Voltages(PhaseCode thesePhases) 
        {
            phasesPresent = thesePhases;
            phaseVoltages = new Dictionary<SinglePhaseType, Voltage>();
            var phaseArray = thesePhases.ToSinglePhaseTypeList();
            foreach (SinglePhaseType phase in phaseArray)
            {
                phaseVoltages[phase] = new Voltage(this, phase);
            }
        }

        /// <summary>
        /// Backing collection for the contained Voltage objects
        /// </summary>
        private Dictionary<SinglePhaseType, Voltage> phaseVoltages;
        private PhaseCode phasesPresent;

        /// <summary>
        /// The phases defined for a Voltages instance
        /// </summary>
        public PhaseCode PhasesPresent { get { return phasesPresent; } }

        /// <summary>
        /// Updates the values for the Voltage of the given phase
        /// </summary>
        /// <param name="thisPhase"></param>
        /// <param name="thisMagnitude"></param>
        /// <param name="thisAngle"></param>
        public void SetValues(SinglePhaseType thisPhase, double thisMagnitude, double thisAngle)
        {
            phaseVoltages[thisPhase].Magnitude = thisMagnitude;
            phaseVoltages[thisPhase].Angle = thisAngle;
        }

        /// <summary>
        /// Returns an array of all of the voltages that correspond to the flags present in the given PhaseCode
        /// </summary>
        /// <param name="phases"></param>
        /// <returns></returns>
        public Voltage[] this[PhaseCode phases]
        {
            get 
            {
                var phaseList = phases.ToSinglePhaseTypeList();
                var voltageArray = new Voltage[phaseList.Count];
                for (int i = 0; i < phaseList.Count; i++)
                {
                    voltageArray[i] = phaseVoltages[phaseList[i]];
                }
                return voltageArray;
            }
            
        }

        /// <summary>
        /// Returns a string giving the angle and magnitude values for all phases
        /// </summary>
        /// <returns></returns>
        public string PrintAllValues()
        {
            var printOut = new StringBuilder();
            foreach (KeyValuePair<SinglePhaseType, Voltage> phaseVoltage in phaseVoltages)
            {
                printOut.AppendFormat
                    ("Phase{0} Magnitude = {1}", phaseVoltage.Key.ToString(), phaseVoltage.Value.Magnitude.ToString());
                printOut.AppendLine();
                printOut.AppendFormat
                    ("Phase{0} Angle = {1}", phaseVoltage.Key.ToString(), phaseVoltage.Value.Angle.ToString());
                printOut.AppendLine();
            }
            return printOut.ToString();

        }

        /// <summary>
        /// The voltage magnitude and angle for a phase 
        /// </summary>
        public class Voltage : IdentifiedObject
        {
            private Voltages parent;
            private SinglePhaseType phase;


            public double Magnitude { get; set; }
            public double Angle { get; set; }
            public SinglePhaseType Phase { get { return phase; } }

            public Voltage(Voltages thisParent, SinglePhaseType thisPhase) 
            {
                this.phase = thisPhase;
                this.parent = thisParent;  
            }

        }
    }
}
