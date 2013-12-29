using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using ModelManipulation;

namespace IdentifiedObjects
{
    class MeshPowerflow : IPowerFlow
    {

        #region IPowerFlow Implementation
        public MeshPowerflow() { }

        /// <summary>
        /// Provides the powerflow program with adequate information to construct the Ybus matrix for the power system
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="conductors"></param>
        /// <param name="sourceNode"></param>
        /// <param name="?"></param>
        public void NewACFeederCircuit(Line newFeeder)
        { }

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
        { return false; }

        #endregion

        //Unbalanced Ybus matrix, with transforms for all other 
        private Complex[][] Ytransform;
        private Line feeder;
        //builds in reverse order of BFS to decrease bandwidth
        private void buildTmatrix()
        {
            //The dimensions of the matrix must include all three phases for all DynamicTopologicalNodes
            var length = feeder.DynamicTopologicalNodes.Count * 3;
            Ytransform = new Complex[length][];
            var nextNodes = new Queue<TopologicalNode.DynamicTopologicalNode>();
            TopologicalNode.DynamicTopologicalNode currentNode;
            var visitedNodes = new Stack<TopologicalNode.DynamicTopologicalNode>();
            nextNodes.Enqueue(feeder.ConnectivityNodes[feeder.bus1].DynamicTopoNode);
            while (nextNodes.Count != 0)
            {
                currentNode = nextNodes.Dequeue();
                foreach (Terminal term in currentNode.ConnectedBranches(true))
                {
                    foreach(SinglePhaseType phase in term.Phases.ToSinglePhaseTypeList())
                    {

                    }
                }
            }

        }


    }
}
