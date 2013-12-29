using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Diagnostics;
using ModelManipulation;

namespace IdentifiedObjects
{
    public class LadderPowerFlow : IPowerFlow
    {
        public LadderPowerFlow()
        { 
        }

        public void NewACFeederCircuit(Line newFeeder)
        {
            feeder = newFeeder;
            buildLadder(feeder);
        }

        /// <summary>
        /// Indicates the current Line loaded into the power flow solution software
        /// </summary>
        /// <returns></returns>
        public Line ReturnActiveFeeder() { return feeder; }


        /// <summary>
        /// RunPowerFlow updates the voltage fields for each ConnectivityNode in the current Line object; returns true if successful 
        /// </summary>
        /// <param name="currentLoads"></param>
        public bool RunPowerFlow(IEnumerable<EnergyConsumer> currentLoads)
        {
            int iterations = 0;
            var error = (float)100;
            while (error > .0001)
            {
                if (iterations > 10)
                { return false; }
                backwardSweep();
                error = forwardSweep();
                iterations++;
            }
            Debug.WriteLine(String.Format("Total Iterations = {0}", iterations.ToString()));
            foreach (TransmissionUnit unit in ladder)
            {
                unit.WriteVoltage();
            }
            return true;
 
        }


        private Line feeder;
        private LinkedList<TransmissionUnit> ladder;
        /// <summary>
        /// Builds a linked list of transmissionUnits, over which the powerflow solver can sweep. 
        /// </summary>
        /// <param name="newLine"></param>
        private void buildLadder(Line newLine)
        {
            ladder = new LinkedList<TransmissionUnit>();
            var sourceUnit = new TransmissionUnit(newLine.ConnectivityNodes[newLine.bus1],newLine.BaseKV);
            var callStack = new Stack<TransmissionUnit>();
            var visited = new Stack<TopologicalNode.DynamicTopologicalNode>();
            callStack.Push(sourceUnit);
            TransmissionUnit currentUnit;
            TransmissionUnit newUnit;
            while (callStack.Count != 0)
            {
                currentUnit = callStack.Pop();
                ladder.AddLast(currentUnit);
                if (visited.Contains(currentUnit.Node))
                {
                    //Since the ladder powerflow only works for radial networks, as implemented.
                    throw new Exception("There was a loop in the feeder");
                }
                foreach (Terminal term in currentUnit.DownstreamTerms())
                {
                    newUnit = new TransmissionUnit(term, currentUnit);
                    callStack.Push(newUnit);
                }
                visited.Push(currentUnit.Node);
            }

        }

        private void backwardSweep()
        {
            var currentNode = ladder.Last;
            while (currentNode != null)
            {
                currentNode.Value.UpdateCurrents();
                currentNode = currentNode.Previous;
            }
        }
        private float forwardSweep()
        {
            var currentNode = ladder.First;
            currentNode = currentNode.Next;
            float repError = 0;
            while (currentNode != null)
            {
                repError = Math.Max(currentNode.Value.UpdateVoltages(), repError);
                currentNode = currentNode.Next;
            }
            return repError;
        }
        //Acts as a single node in the "Ladder"        
        private class TransmissionUnit
        {
            public float BaseKV { get; private set; }
            public Complex[] I{private set;get;}
            public Complex[] V { private set; get; }
            private PowerTransformer.TransformerEnd tbranch;
            private ACLineSegment lineBranch;
            internal TopologicalNode.DynamicTopologicalNode Node { get; private set; }
            private Terminal upstreamTerm;
            private string typeOfBranch;

            internal TransmissionUnit(ConnectivityNode sourceBus, float baseKV)
            {
                DownstreamUnits = new HashSet<TransmissionUnit>();
                Node = sourceBus.DynamicTopoNode;
                V = new Complex[]{baseKV ,baseKV  ,baseKV};
                I = new Complex[]{ 0, 0, 0 };
                BaseKV = baseKV;
            }
            internal TransmissionUnit(Terminal linkPoint, TransmissionUnit powerSource)
            {
                I = new Complex[] { 0, 0, 0 };
                UpstreamUnit = powerSource;
                DownstreamUnits = new HashSet<TransmissionUnit>();
                powerSource.DownstreamUnits.Add(this);
                var tryT = linkPoint.ParentEquipment as PowerTransformer.TransformerEnd;
                var tryLine = linkPoint.ParentEquipment as ACLineSegment;
                //For now assume that Ends[0] is always the upstream TransformerEnd
                if (tryT != null) 
                {
                    typeOfBranch = "TransformerEnd";
                    tbranch = tryT;
                    upstreamTerm = tryT.ParentTransformer.Ends[1].Terminal1;
                    Node = upstreamTerm.ConnectionPoint.DynamicTopoNode;
                    BaseKV = UpstreamUnit.BaseKV * 
                        (tryT.ParentTransformer.Ends[1].BaseKV / tryT.ParentTransformer.Ends[0].BaseKV);  
                }
                if (tryLine != null)
                {
                    typeOfBranch = "Conductor";
                    lineBranch = tryLine;
                    BaseKV = UpstreamUnit.BaseKV;
                    if (ReferenceEquals(tryLine.Terminal1, linkPoint))
                    {
                        Node = tryLine.Terminal2.ConnectionPoint.DynamicTopoNode;
                        upstreamTerm = tryLine.Terminal2;
                    }
                    if (ReferenceEquals(tryLine.Terminal2, linkPoint))
                    {
                        Node = tryLine.Terminal1.ConnectionPoint.DynamicTopoNode;
                        upstreamTerm = tryLine.Terminal1;
                    }
                }
                V = new Complex[]{BaseKV,BaseKV,BaseKV};
            }

            public TransmissionUnit UpstreamUnit { get; set; }
            public HashSet<TransmissionUnit> DownstreamUnits { get; set; }
            public IEnumerable<Terminal> DownstreamTerms()
            {
                return Node.ConnectedBranches(true).Where(x => !object.ReferenceEquals(upstreamTerm, x));
            }

            public void UpdateCurrents()
            {
                for (int i = 0; i < 3; i++ )
                {
                    I[i] = 0;
                }
                foreach (TransmissionUnit unit in DownstreamUnits)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        I[i] = I[i] + unit.I[i];  
                    }
                }
                foreach (Terminal term in Node.CurrentInjectors(false))
                {
                    var load = term.ParentEquipment as EnergyConsumer;
                    if (load != null)
                    {
                        var currents = load.Currents(V,BaseKV);
                        for (int i = 0; i < 3; i++)
                        {
                            I[i] = I[i] + currents[i];
                        }
                    }
                }
            }

            public float UpdateVoltages()
            {
                if (typeOfBranch == "Conductor")
                {
                    var newV = updateLineVoltages();
                    float error = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        error = Math.Max(error, Math.Abs((float)(newV[i] - V[i]).Real)); 
                    }
                    V = newV;
                    return error;
                }
                if (typeOfBranch == "TransformerEnd")
                {
                    //Just until I implement the transformer model, which will be a little more complicated.
                    if (tbranch.ConnectionType == PhaseShuntConnectionKind.D)
                    {
                        if (tbranch.ParentTransformer.Ends[1].ConnectionType == PhaseShuntConnectionKind.D)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                V[i] = UpstreamUnit.V[i] * tbranch.ParentTransformer.TurnsRatio();
                            }
                        }
                        if (tbranch.ParentTransformer.Ends[1].ConnectionType == PhaseShuntConnectionKind.Y)
                        {
                            V = Transforms.VLLtoLN(UpstreamUnit.V, tbranch.ParentTransformer.TurnsRatio());
                        }
                    }
                    if (tbranch.ConnectionType == PhaseShuntConnectionKind.Y)
                    {
                        if (tbranch.ParentTransformer.Ends[1].ConnectionType == PhaseShuntConnectionKind.Y)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                V[i] = UpstreamUnit.V[i] * tbranch.ParentTransformer.TurnsRatio();
                            }
                        }
                        if (tbranch.ParentTransformer.Ends[1].ConnectionType == PhaseShuntConnectionKind.D)
                        {
                            V = Transforms.VLNtoLL(UpstreamUnit.V, tbranch.ParentTransformer.TurnsRatio());
                        }
 
                    }
                }
                return 0;
            }

            public void WriteVoltage()
            {
                foreach (ConnectivityNode thisNode in Node.Nodes.ConnectivityNodes.Values)
                {
                    if (thisNode.NodeVoltages.PhasesPresent.HasFlag(PhaseCode.A))
                    {
                        thisNode.NodeVoltages.SetValues(SinglePhaseType.A, (float)V[0].Magnitude , (float)V[0].Phase);
                    }
                    if (thisNode.NodeVoltages.PhasesPresent.HasFlag(PhaseCode.B))
                    {
                        thisNode.NodeVoltages.SetValues(SinglePhaseType.B, (float)V[1].Magnitude , (float)V[1].Phase);
                    }
                    if (thisNode.NodeVoltages.PhasesPresent.HasFlag(PhaseCode.C))
                    {
                        thisNode.NodeVoltages.SetValues(SinglePhaseType.C, (float)V[2].Magnitude , (float)V[2].Phase);
                    }
                }
            }
            private Complex[] updateLineVoltages()
            {
                var Vup = UpstreamUnit.V;
                Complex[][] Z = lineBranch.PhaseImpedance();
                var newV = new Complex[3];
                for (int i = 0; i < 3; i++)
                {
                    newV[i] = Vup[i];
                    for (int j = 0; j < 3; j++)
                    {
                        newV[i] = newV[i] - (Z[i][j] * I[j]) / 1000;
                    }
                }
                return newV;
            }
        }

    }
}
