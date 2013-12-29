using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenDSSengine;
using System.Reflection;
using System.IO;
using System.Numerics;


namespace IdentifiedObjects
{
    /// <summary>
    /// Used to run OpenDSS to obtain powerflow solutions 
    /// </summary>
    public class OpenDSSPowerFlow : IPowerFlow
    {
        public OpenDSSPowerFlow()
        {
            currentDSS = new OpenDSSengine.DSS();
            dssText = currentDSS.Text;
            commandMakers = new Dictionary<string, Func<IdentifiedObject, string>>();
            commandMakers["ACLineSegment"] = x => LineToDSS((ACLineSegment)x);
            commandMakers["LineModel"] = x => lineModelToDSS((LineModel)x);
            commandMakers["Switch"] = x => null; //switches should be optimized out before running powerflow.
            commandMakers["ShuntCompensator"] = x => compensatorToDSS((ShuntCompensator)x);
            commandMakers["PowerTransformer"] = x => transformerToDSS((PowerTransformer)x);
            commandMakers["TransformerEnd"] = x => null;//transformer ends are accounted for in the transformerToDSS method
            writeScriptFile = false;
        }

        public OpenDSSPowerFlow(string newFilePath)
        {
            currentDSS = new OpenDSSengine.DSS();
            dssText = currentDSS.Text;
            commandMakers = new Dictionary<string, Func<IdentifiedObject, string>>();
            commandMakers["ACLineSegment"] = x => LineToDSS((ACLineSegment)x);
            commandMakers["LineModel"] = x => lineModelToDSS((LineModel)x);
            commandMakers["Switch"] = x => null; //switches should be optimized out before running powerflow.
            commandMakers["ShuntCompensator"] = x => compensatorToDSS((ShuntCompensator)x);
            commandMakers["PowerTransformer"] = x => transformerToDSS((PowerTransformer)x);
            commandMakers["TransformerEnd"] = x => null;//transformer ends are accounted for in the transformerToDSS method
            writeScriptFile = true;
            scriptFilePath = newFilePath;
        }

        private StreamWriter scriptFileWriter;
        private string scriptFilePath;
        private bool writeScriptFile;
        private OpenDSSengine.DSS currentDSS;
        private OpenDSSengine.Text dssText;
        private Line feeder;
        private string sourceBusName;
        public ConnectivityNode SourceBus
        {
            set
            {
                sourceBusName = value.Name;
            }
        }

        public Line ReturnActiveFeeder() { return feeder; }
       
        
        public void NewACFeederCircuit(Line newFeeder)
        {
            feeder = newFeeder;
            if (writeScriptFile)
            {
                scriptFileWriter = new StreamWriter(scriptFilePath +"\\" +feeder.Name +".dss");
            }
            writeCommand("Clear");
            sourceBusName = newFeeder.bus1;
            writeCommand(newDSSCircuit());
            foreach (LineModel model in ACLineSegment.LineModels.Values)
            {
                writeCommand(lineModelToDSS(model));
            }
            foreach (Equipment gear in feeder)
            {
                //treat energyconsumer seperately since each load may require multiple command strings
                if (gear.Type == "EnergyConsumer")
                {
                    foreach (string mystring in LoadToDSS((EnergyConsumer)gear))
                    {
                        writeCommand(mystring);
                    }
                }
                else
                {
                    writeCommand(commandMakers[gear.Type](gear));
                }
    
            }
            if (writeScriptFile)
            {
                scriptFileWriter.Close();
            }
        }

        public bool RunPowerFlow(IEnumerable<EnergyConsumer> currentLoads)
        {
            //Update the Real and reactive power consumption at each load to match the values for the current time
            foreach (EnergyConsumer load in currentLoads)
            {
                dssText.Command = String.Format("Load.{0}.Kw = {1}", load.Name, load.Pfixed);
                dssText.Command = String.Format("Load.{0}.Kvar = {1}", load.Name, load.Qfixed);
            }

            //Perform a single powerflow solution for the current circuit

            dssText.Command = "Solve";
            


            //If powerflow did not converge, exit method. Good place to check while debugging...
            if (currentDSS.ActiveCircuit.Solution.Converged == false) { return false; }

            //The return of the method is in the Delphi type of "OleVariant Array", 
            var pfSolutionDelphi = currentDSS.ActiveCircuit.AllBusVolts;
            var nodeNamesDelphi = currentDSS.ActiveCircuit.AllNodeNames;

            //Attempted cast - very likely that this will fail due to type ambiguity
            var pfSolution = pfSolutionDelphi   as Complex[];
            var nodeNames  = nodeNamesDelphi    as String[] ;

            if ((pfSolution != null) && (nodeNames != null))
            {
                //Update all the node voltages in the current Line with the results of the powerflow
                return updateVoltages(nodeNames, pfSolution);
            }

            return false;
        }

       


        #region Private Methods
        
        /// <summary>
        /// Using the returned values from the OpenDSS powerflow, attempts to update 
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private bool updateVoltages(String[] nameArray,Complex[] complexVoltageArray)
        {
            //Index for enumerating the values in the complex voltage array
            int index = 0;
            //A period is the delimiter used by OpenDSS to seperate the bus name from the node number
            char[] breakPoint = {'.'};

            foreach (String nodeName in nameArray)
            {
                //Split the nodeName string at the period (ex. "Bus1.1" => {"Bus1","1"}), crude parsing
                string[] nameAndPhase = nodeName.Split(breakPoint);
                double phaseNum = Convert.ToInt16(nameAndPhase[1]);

                /* Attempting to switch to the correct int for each enum value - since this is questionable,
                please fix if you have a better solution... */
                SinglePhaseType phase = (SinglePhaseType)(int)Math.Pow((double)2, phaseNum - (double)1);
                float voltageMag   = (float)complexVoltageArray[index].Magnitude;
                float voltageAngle = (float)complexVoltageArray[index].Phase;
                feeder.ConnectivityNodes[nameAndPhase[0]].NodeVoltages.SetValues(phase, voltageMag, voltageAngle);
                index++;
            }
            //May implement further checks before ending method if errors are common
            return true;
        }

        /// <summary>
        /// Used to write a command to a stream, leading to a script file
        /// </summary>
        /// <param name="comm"></param>
        private void writeCommand(string comm)
        {
            if(writeScriptFile)
            {
                scriptFileWriter.WriteLine(comm);
            }
            dssText.Command = comm;
        }

        /// <summary>
        /// provides a look-up table for command-generating methods. EnergyConsumer is not included becuase it needs to return multiple strings
        /// </summary>
        private static Dictionary<string, Func<IdentifiedObject, string>> commandMakers;
        

        /// <summary>
        /// Converts a LineModel to a definition for a LineCode in OpenDSS
        /// </summary>
        /// <param name="linemodel"></param>
        /// <returns></returns>
        private string lineModelToDSS(LineModel linemodel) 
        {
            var comm = new StringBuilder("new linecode");
            comm.AppendFormat(".{0} nphases={1} BaseFreq=60 units=ft", linemodel.Name, linemodel.Phasing.NumPhases());
            comm.AppendFormat(" rmatrix=({0}|{1} {2} | {3} {4} {5}) ", linemodel.R[0][0], linemodel.R[0][1], linemodel.R[1][0],
                linemodel.R[0][2], linemodel.R[1][1], linemodel.R[2][0]);
            comm.AppendFormat(" xmatrix=({0}|{1} {2} | {3} {4} {5}) ", linemodel.X[0][0], linemodel.X[0][1], linemodel.X[1][0],
                linemodel.X[0][2], linemodel.X[1][1], linemodel.X[2][0]);
            //C is a method rather than property because it is converted from B (susceptance) rather than being stored
            comm.AppendFormat(" cmatrix=({0}|{1} {2} | {3} {4} {5}) ", linemodel.C(0,0), linemodel.C(0,1), linemodel.C(1,0),
                linemodel.C(0,2), linemodel.C(1,1), linemodel.C(2,0));
            return comm.ToString();
        }
        /// <summary>
        /// Returns a command for initializing a new OpenDSS circuit
        /// </summary>
        /// <returns></returns>
        private string newDSSCircuit()
        {
            var circuitDeclaration = new StringBuilder("new Circuit.");
            circuitDeclaration.Append(feeder.Name);
            circuitDeclaration.AppendFormat(" Bus1={0}", sourceBusName);
            circuitDeclaration.Append(" Phases=3");
            return circuitDeclaration.ToString();
        }



        /// <summary>
        /// Generates an OpenDSS definition for a line segment
        /// </summary>
        /// <param name="newLineSeg"></param>
        /// <returns></returns>
        /// 
        private string LineToDSS(ACLineSegment newLineSeg)
        {
            StringBuilder dssLine = new StringBuilder("new Line");
            dssLine.AppendFormat(".{0}", newLineSeg.Name);
            dssLine.Append(ToDSSBusFormat(newLineSeg.Terminal1,newLineSeg.Terminal2));
            dssLine.AppendFormat(" LineCode={0}", newLineSeg.LineModel.Name);
            dssLine.AppendFormat(" Length={0} ", newLineSeg.Length.ToString());
            return dssLine.ToString();
        }

        /// <summary>
        /// Returns the OpenDSS definition for a capacitor or a 
        /// </summary>
        /// <param name="shunt"></param>
        /// <returns></returns>
        private string compensatorToDSS(ShuntCompensator shunt)
        {
            StringBuilder comm;
            if (shunt.B < 0)
            {
                comm = new StringBuilder("new cap");
                comm.AppendFormat(" x={0} ", 1 / (shunt.B));
                comm.AppendFormat(" r={0} ", 1 / (shunt.C));
                comm.Append(ToDSSBusFormat(shunt.Terminal1));
            }
            else
            {
                comm = new StringBuilder("new reactor");
                comm.AppendFormat(" x={0} ",1/(shunt.B));
                comm.AppendFormat(" r={0} ", 1 / (shunt.C));
                comm.Append(ToDSSBusFormat(shunt.Terminal1));
            }

            return comm.ToString();
        }


        /// <summary>
        /// Generates an OpenDSS definition for a load
        /// </summary>
        /// <param name="newLoad"></param>
        /// <returns></returns>
        private IEnumerable<string> LoadToDSS(EnergyConsumer newLoad)
        {
            var comms = new List<string>();
            if (newLoad.LoadResponse.PConstantPower != 0 || newLoad.LoadResponse.QConstantPower != 0)
            {
                comms.Add(subLoadToDSS(newLoad,newLoad.LoadResponse.PConstantPower,newLoad.LoadResponse.QConstantPower, 1)); 
            }
            if (newLoad.LoadResponse.PConstantImpedance != 0 || newLoad.LoadResponse.QConstantImpedance != 0)
            {
                comms.Add(subLoadToDSS(newLoad,newLoad.LoadResponse.PConstantImpedance,newLoad.LoadResponse.QConstantImpedance,2));
            }
            if (newLoad.LoadResponse.PConstantCurrent != 0  || newLoad.LoadResponse.QConstantCurrent != 0)
            {
                comms.Add(subLoadToDSS(newLoad, newLoad.LoadResponse.PConstantCurrent,newLoad.LoadResponse.QConstantCurrent,5));
            }
            return comms;
        }
            

        /// <summary>
        /// Once a load is broken up between multiple models, this returns the command for initializing loads for each model
        /// </summary>
        /// <param name="newLoad"></param>
        /// <param name="pmult"></param>
        /// <param name="qmult"></param>
        /// <param name="loadnum"></param>
        /// <returns></returns>
        private string subLoadToDSS(EnergyConsumer newLoad, float pmult, float qmult, int loadnum)
        {
            Dictionary<PhaseShuntConnectionKind, string> conn = new Dictionary<PhaseShuntConnectionKind, string>();
            conn.Add(PhaseShuntConnectionKind.D, "Delta");
            conn.Add(PhaseShuntConnectionKind.Y, "Wye");
            StringBuilder dssLoad = new StringBuilder("New Load");
            dssLoad.AppendFormat(".{0}", newLoad.Name);
            dssLoad.Append(ToDSSBusFormat(newLoad.Terminal1));
            dssLoad.AppendFormat(" model={0}", loadnum.ToString());
            dssLoad.AppendFormat(" conn={0} ", conn[newLoad.PhaseConnection]);
            dssLoad.AppendFormat(" kW={0} kVar={1}", (newLoad.Pfixed * pmult).ToString(), (newLoad.Qfixed * qmult).ToString());
            return dssLoad.ToString();
        }
        private string transformerToDSS(PowerTransformer tran)
        {
            StringBuilder dssTran = new StringBuilder("New Transformer");
            dssTran.AppendFormat(".{0}  ", tran.Name);
            int endCount = 0;
            int phaseCount = 0;
            var windingDefs = new StringBuilder();
            PowerTransformer.TransformerEnd highest = null;
            PowerTransformer.TransformerEnd lowest = null;
            foreach (PowerTransformer.TransformerEnd end in tran.Ends)
            {
                endCount++;
                if(highest == null || highest.BaseKV < end.BaseKV)
                {
                    highest = end;
                }
                if(lowest == null || lowest.BaseKV > end.BaseKV)
                {
                    lowest = end;
                }
                phaseCount = Math.Max(phaseCount,end.Terminal1.Phases.NumPhases());
                windingDefs.AppendFormat
                    (
                    " wdg={0} bus={1}   ",
                    endCount.ToString(),
                    end.Terminal1.ConnectionPointName
                    );
                windingDefs.AppendFormat
                    (
                    "conn={0} kv={1}  ",
                    end.ConnectionType.ToString(),
                    end.BaseKV.ToString()
                    );
            }
            dssTran.AppendFormat
                (
                "Windings={0} Phases={1} ",
                endCount.ToString(),
                phaseCount
                );
            dssTran.AppendFormat
                (
                " Xhl={0} kVA={1}",
                tran.Xpercentage(highest, lowest).ToString(),
                tran.BaseKVA.ToString()
                );
            return dssTran.Append(windingDefs.ToString()).ToString();


        }
        

        /// <summary>
        /// ToDSSBusFormat generates the string defining the busses and number of phases for each OpenDSS conducting equipment.
        /// </summary>
        /// <param name="newTerm"></param>
        /// <returns></returns>
        private string ToDSSBusFormat(Terminal newTerm)
        {
            return ToDSSBusFormat(newTerm, 1, true);
        }
        /// <summary>
        /// The Two Terminal overload only gives the phase count once
        /// </summary>
        /// <param name="newTerm1"></param>
        /// <param name="newTerm2"></param>
        /// <returns></returns>
        private string ToDSSBusFormat(Terminal newTerm1, Terminal newTerm2)
        {
            StringBuilder TermToBus = new StringBuilder(ToDSSBusFormat(newTerm1, 1, false));
            TermToBus.Append(ToDSSBusFormat(newTerm2,2,true));
            return TermToBus.ToString();
        }
        private string ToDSSBusFormat(Terminal newTerm, int busNumber, bool returnPhaseCount)
        {
            int phaseCount = 0;
            StringBuilder termToBus = new StringBuilder(" bus");

            //The bus name is the name of the topological node associated with newTerm's connectivitynode.
            termToBus.AppendFormat("{0} = {1}", busNumber.ToString(), newTerm.ConnectionPoint.DynamicTopoNode.Name); 
            if (newTerm.Phases.HasFlag(PhaseCode.A))
            {
                termToBus.Append(".1");
                phaseCount++;
            }
            if (newTerm.Phases.HasFlag(PhaseCode.B))
            {
                termToBus.Append(".2");
                phaseCount++;
            }
            if (newTerm.Phases.HasFlag(PhaseCode.C))
            {
                termToBus.Append(".3");
                phaseCount++;
            }
            if (returnPhaseCount)
            {
                termToBus.AppendFormat(" Phases = {0}", phaseCount.ToString());
            }
            return termToBus.ToString();
        }

        #endregion

    }
}
