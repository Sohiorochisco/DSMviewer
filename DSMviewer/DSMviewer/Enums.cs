using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IdentifiedObjects
{
    /// <summary>
    /// Categorizes each conductingEquipment to allow sorting and selection without reflection
    /// </summary>
    public enum EquipmentTopoTypes
    {
        None            = 0,
        Conductor       = 1,
        Switch          = 2,
        Shunt           = 3
    }


    /// <summary>
    /// Indicates whether a switch is open or closed.
    /// </summary>
    public enum SwitchState
    {
        open = 1,
        closed = 2
    }
    /// <summary>
    /// PhaseCode specifies the phases present for each terminal
    /// </summary>
    [Flags]
   public enum PhaseCode : int
    {
        None = 0,
        A = 1,
        B = 2,
        C = 4,
        N = 8
    }


    /// <summary>
    /// SinglePhaseType replaces PhaseCode in situations where only a single phase may be specified.
    /// </summary>
    public enum SinglePhaseType : int
    {
        A = 1,
        B = 2,
        C = 4,
        N = 0
    }

    
    

    /// <summary>
    /// Gives the connection type for the load.
    /// </summary>
    public enum PhaseShuntConnectionKind
    {
        D   =   1,
        Y   =   2, 
        Yn  =   3,
        I   =   4
    }

    public enum SynchronousMachineType
    {
        generator = 1,
        condenser = 2,
        generator_or_condenser = 3
    }

    public enum SynchronousMachineOperatingMode
    {
        generator = 1,
        condenser = 2
    }

    public enum TransformerControlMode
    {
        volt = 1,
        reactive = 2
    }

    public enum TapChangerKind
    {
        isfixed = 1,
        voltageControl = 2,
        phaseControl = 3,
        voltageAndPhaseControl = 4
    }

    [Flags]
    public enum RegulatingControlModeKind
    {
        voltage = 1,
        activePower = 2,
        reactivePower = 4,
        currentFlow = 8,
        //differs from CIM because "fixed" is a reserved keyword in C#
        isfixed = 16, 
        admittance = 32,
        timeScheduled = 64,
        temperature = 128,
        powerFactor = 256
    }

    public enum BreakerConfiguration
    {
        singleBreaker = 1,
        breakerAndAHalf = 2,
        doubleBreaker = 3,
        noBreaker = 4
    }

    public enum BusbarConfiguration
    {
        singleBus = 1,
        doubleBus = 2,
        mainWithTransfer = 3,
        ringBus = 4
    }
    
    public static class EnumExtentions
    {
        //Probably redundant; HasFlags seems to perform the same task. Will remove once this is established
        /// <summary>
        /// The Has extention method returns whether the PhaseCode contains a particular combination of phases
        /// </summary>
        /// <param name="phases"></param>
        /// <param name="certainPhase"></param>
        /// <returns></returns>
        public static bool Has(this PhaseCode phases, PhaseCode certainPhase)
        {
            return (phases & certainPhase) == phases;
        }
        /// <summary>
        /// returns the number of phases present in a phasecode, excluding neutral 
        /// </summary>
        /// <param name="phases"></param>
        /// <returns></returns>
        public static int NumPhases(this PhaseCode phases)
        {
            int i = 0;
            if (phases.HasFlag(PhaseCode.A))
            {i++;}
            if (phases.HasFlag(PhaseCode.B))
            { i++; }
            if (phases.HasFlag(PhaseCode.C))
            { i++; }
            return i;
        }
        /// <summary>
        /// Converts a PhaseCode to a list of SinglePhaseTypes
        /// </summary>
        /// <param name="phases"></param>
        /// <returns></returns>
        public static List<SinglePhaseType> ToSinglePhaseTypeList(this PhaseCode phases)
        {
            var list = new List<SinglePhaseType>();
            foreach (SinglePhaseType phase in Enum.GetValues(typeof(SinglePhaseType)).Cast<SinglePhaseType>())
            {
                //converting between the two enum types.
                PhaseCode thisPhase = (PhaseCode)phase;
                if (phases.HasFlag(thisPhase))
                {
                    list.Add(phase);
                }
            }

            return list;
        }

       
    }

}
