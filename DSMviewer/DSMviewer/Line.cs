using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ModelManipulation;
using System.Data;

namespace IdentifiedObjects
{
    /// <summary>
    /// Equipment container for an entire feeder
    /// </summary>
    public class Line : EquipmentContainer 
    {
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        internal float BaseKV{get;private set;}
        public SubGeographicalRegion Area { get; private set; }
        internal string bus1 { get; set; }
        protected IPowerFlow powerflow;
        /// <summary>
        /// Flag to indicated that the Line has been changed
        /// </summary>
        internal bool Revised { get; set; }
        internal List<TopologicalNode.VoltageLevel> VoltageLevels { get; private set; }
        internal List<TopologicalNode.DynamicTopologicalNode> DynamicTopologicalNodes { get; private set; }
        private LineBuilder lineBuilder;
        internal int UpdateLoads(DateTime t)
        {
            var loadCount = lineBuilder.UpdateLoads(t);
            foreach (TopologicalNode.VoltageLevel node in VoltageLevels)
            {
                makeDynamicTopologicalNodes(node, t);
            }
            return loadCount;
        }

        internal void SetPowerFlow(IPowerFlow thisPowerFlow)
        {
            powerflow = thisPowerFlow;
        }


            

        #region private methods

        private HashSet<TopologicalNode.DynamicTopologicalNode> makeDynamicTopologicalNodes
            (TopologicalNode.VoltageLevel parentNode, DateTime timeIndex)
        {
            var dynaNodes = new HashSet<TopologicalNode.DynamicTopologicalNode>();
            foreach(KeyValuePair<string,ConnectivityNode> connNode in parentNode.Nodes.ConnectivityNodes)
            {
                if (connNode.Value.DynamicTopoNode == null)
                {
                    var dynaNode = new TopologicalNode.DynamicTopologicalNode(connNode.Value, parentNode, timeIndex);
                    connNode.Value.DynamicTopoNode = dynaNode;
                    dynaNodes.Add(dynaNode);
                    traverseClosedSwitches(dynaNode, connNode.Value);
                }
                if(connNode.Value.DynamicTopoNode.TimeIndex != timeIndex)
                {
                    if(!parentNode.DynamicNodes.ContainsKey(timeIndex))
                    {
                        parentNode.DynamicNodes[timeIndex] = new HashSet<TopologicalNode.DynamicTopologicalNode>();
                    }
                    var dynaNode = new TopologicalNode.DynamicTopologicalNode(connNode.Value,parentNode,timeIndex);
                    connNode.Value.DynamicTopoNode = dynaNode;
                    parentNode.DynamicNodes[timeIndex].Add(dynaNode);
                    traverseClosedSwitches(dynaNode, connNode.Value);
                    dynaNodes.Add(dynaNode);     

                }
                
            }
            return dynaNodes;

        }
        
        private void traverseClosedSwitches(TopologicalNode.DynamicTopologicalNode dynaNode, ConnectivityNode connNode)
        {
            var switchNodes = new Stack<ConnectivityNode>();
            switchNodes.Push(connNode);
            while (switchNodes.Count != 0)
            {
                var currentNode = switchNodes.Pop();
                dynaNode.Nodes.ConnectivityNodes[currentNode.Name] = currentNode;
                currentNode.DynamicTopoNode = dynaNode;
                foreach (ConnectivityNode nextConnNode in currentNode.AdjacentSwitchNodes(false)
                    .Where(x => !ReferenceEquals(x,currentNode)))
                {
                    switchNodes.Push(nextConnNode);
                }
            }
        }
        
        #endregion

        
        private class LineBuilder : ModelBuilder
        {
            public LineBuilder(Line parentLine) 
                :base(parentLine)
            {
                filterField = "Line";
                containedTypeNames = new string[] 
                { 
                    "Terminal",
                    "ACLineSegment", 
                    "EnergyConsumer", 
                    "PowerTransformer",
                    "PowerTransformerEnd",
                    "Switch"
                };
            }

            Line lineObject;

            internal int UpdateLoads(DateTime t)
            {
                return datasource.GetTimestepLoadData(t,updateALoad);
            }
            protected bool updateALoad(DateTime t, string name, float Pmult, float Qmult)
            {
                var load = lineObject[name] as EnergyConsumer;
                if (load == null) { return false; }
                load.Pmult = Pmult;
                load.Qmult = Qmult;
                return true;
            }

            protected override void constructAndConnect(Dictionary<string, DataTable> typesToBuild)
            {
                var terms = new Dictionary<string, Terminal>();
                lineObject = modelObject as Line;
                if (typesToBuild["PowerTransformer"] != null)
                {
                    foreach (DataRow row in typesToBuild["PowerTransformer"].Rows)
                    {
                        var tranName = (string)row["Name"];
                        lineObject[tranName] = new PowerTransformer(row);
                    }
                    foreach (DataRow row in typesToBuild["PowerTransformerEnd"].Rows)
                    {
                        var endName = (string)row["Name"];
                        var parent = (PowerTransformer)lineObject[(string)row["PowerTransformer"]];
                        var end = new PowerTransformer.TransformerEnd(row, parent);
                        lineObject[endName] = end;
                        parent.Ends.Add(end);

                    }
                }
                foreach (DataRow row in typesToBuild["Switch"].Rows)
                {
                    var switchName = (string)row["Name"];
                    lineObject[switchName] = ConductingEquipment.newConductingEquipment(row);
                }
                foreach (DataRow row in typesToBuild["ACLineSegment"].Rows)
                {
                    var linesegName = (string)row["Name"];
                    lineObject[linesegName] = ConductingEquipment.newConductingEquipment(row);
                }
                foreach (DataRow row in typesToBuild["EnergyConsumer"].Rows)
                {
                    var loadName = (string)row["Name"];
                    lineObject[loadName] = ConductingEquipment.newConductingEquipment(row);
                }
                foreach (DataRow row in typesToBuild["Terminal"].Rows)
                {
                    var termName = (string)row["Name"];
                    var parentName = (string)row["ParentEquipment"];
                    terms.Add(termName,new Terminal(row,(ConductingEquipment)lineObject[parentName],lineObject.serviceArea));
                }

                //instantiate and connect conducting equipment classes
                makeBranches(terms);
                makeShunts(terms);
                makeStaticTopology(lineObject);
            }

            protected void makeBranches( Dictionary<string,Terminal> terms)
            {      
                foreach (Equipment gear in lineObject.Where(gear => gear.EquipmentType == EquipmentTopoTypes.Conductor))
                {
                    Conductor cond = (Conductor)gear;
                    cond.Terminal1 = terms[cond.Term1Name];
                    cond.Terminal2 = terms[cond.Term2Name];
                    //connect to connectivityNodes
                    addBranchTerminals(cond.Terminal1, cond.Terminal1.ConnectionPointName);
                    addBranchTerminals(cond.Terminal2, cond.Terminal2.ConnectionPointName);
                }
                foreach (Equipment gear in lineObject.Where(gear => gear.EquipmentType == EquipmentTopoTypes.Switch))
                {
                    Switch cond = (Switch)gear;
                    cond.Terminal1 = terms[cond.Term1Name];
                    cond.Terminal2 = terms[cond.Term2Name];
                    //Connect to ConnectivityNodes
                    addBranchTerminals(cond.Terminal1, cond.Terminal1.ConnectionPointName);
                    addBranchTerminals(cond.Terminal2, cond.Terminal2.ConnectionPointName);
                }

            }
            /// <summary>
            /// Creates ConductingEquipment which is either grounded or connected in Delta
            /// </summary>
            /// <param name="shuntVals"></param>
            /// <param name="shuntTermVals"></param>

            protected void makeShunts(Dictionary<string,Terminal> terms)
            {
                
                foreach (Equipment gear in lineObject.Where( gear => gear.EquipmentType == EquipmentTopoTypes.Shunt))
                {
                    var cond = (ConductingEquipment)gear; 
                    var term =  terms[cond.Term1Name];
                    cond.Terminal1 = term;
                    addBranchTerminals(cond.Terminal1, cond.Terminal1.ConnectionPointName);
                }

            }

            private void makeStaticTopology(Line line)
            {
                //Iterates through all connectivitynodes, and checks if they are connected to switches. 
                //if connected, checks if node is already part of a topological node.
                //if not already part of node, performs a breadth-first traversal and adds all switch-connected connectivitynodes to a topologicalnode
                foreach (KeyValuePair<string, ConnectivityNode> node in line.ConnectivityNodes)
                {
                    if (node.Value.StaticTopoNode == null)
                    {
                        var newTopoNode = new TopologicalNode.VoltageLevel();
                        line.VoltageLevels.Add(newTopoNode);
                        node.Value.StaticTopoNode = newTopoNode;
                        traverseSwitches(newTopoNode,node.Value);
                        
                    }
                }
            }
            /// <summary>
            /// performs a traversal of all nodes connected to each other by switches
            /// </summary>t
            /// <param name="topoNode"></param>
            /// <param name="connNode"></param>
            private void traverseSwitches(TopologicalNode.VoltageLevel topoNode, ConnectivityNode connNode)
            {
                if(topoNode.Nodes.ConnectivityNodes.ContainsKey(connNode.Name))
                { return;}
                topoNode.Nodes.ConnectivityNodes.Add(connNode.Name, connNode);
                connNode.StaticTopoNode = topoNode;
                foreach (ConnectivityNode adjacentNode in connNode.AdjacentSwitchNodes(true))
                {
                    traverseSwitches(topoNode, adjacentNode);                    
                }
            }
            /// <summary>
            /// Creates ConnectivityNodes, and associates Terminals of branch conductingEquipment with them
            /// </summary>
            /// <param name="newTerm"></param>
            /// <param name="nodeName"></param>
            protected void addBranchTerminals(Terminal newTerm, string nodeName)
            {
                if (lineObject.ConnectivityNodes.ContainsKey(nodeName))
                {
                    lineObject.ConnectivityNodes[nodeName].Add(newTerm);

                }
                else
                {
                    lineObject.ConnectivityNodes[nodeName] = new ConnectivityNode(nodeName, newTerm);
                    //Console.WriteLine(newTerm.Name);
                }
                //Make the association between Terminal and ConnectivityNode bidirectional by adding the node as a Terminal field
                newTerm.ConnectionPoint = lineObject.ConnectivityNodes[nodeName];
                //Need to add terminal after it is connected, in order to build the Nodes list for the location
                newTerm.Location.AddTerminal(newTerm);
            }
        }

        public Line(string thisName, SubGeographicalRegion thisArea, string sourceBus, float baseKV)
            : base(thisName, thisArea)
        {
            this.lineBuilder = new LineBuilder(this);
            VoltageLevels = new List<TopologicalNode.VoltageLevel>();
            lineBuilder.BuildModel();
            type = "Line";
            bus1 = sourceBus;
            Area = thisArea;
            BaseKV = baseKV;
        }




    }

    
}
