using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    abstract public  class TopologicalNode : IdentifiedObject
    {
    

        
        private ConnectivityNodeContainer nodes;
        public ConnectivityNodeContainer Nodes 
        {
            get 
            {
                return nodes;
            }
            set
            {
                nodes = value;
            }
        }
        public TopologicalNode()    
        {
            nodes = new ConnectivityNodeContainer("topoNodeContainer");
        }

        public class VoltageLevel : TopologicalNode
        {
            private static int staticTopoNodeCount;
            private int dynamicTopoNodeCount;
            public int DynamicTopoNodeCount{get{ return dynamicTopoNodeCount++;}}
            public Dictionary<DateTime,HashSet<TopologicalNode.DynamicTopologicalNode>> DynamicNodes { get; internal set; }
            public VoltageLevel() 
            {
                this.name = "st"+staticTopoNodeCount.ToString();
                staticTopoNodeCount++;
                DynamicNodes = new Dictionary<DateTime, HashSet<DynamicTopologicalNode>>();
            }
            
        }
        /// <summary>
        /// Represents the topologicalNode of each switchset
        /// </summary>
        public class DynamicTopologicalNode : TopologicalNode        
        {
            
            protected VoltageLevel parentStaticNode;
            public VoltageLevel ParentStaticNode{get{return parentStaticNode;}}

            /// <summary>
            /// gives the time-state for which this dynamic node is valid
            /// </summary>
            private DateTime timeIndex;
            /// <summary>
            /// gives the time-state for shich this dynamic node is valid
            /// </summary>
            public DateTime TimeIndex {get{return timeIndex;} }

            //So that making a new union of all terminal collections is not necessary each time ConnectedBranches is called
            private IEnumerable<Terminal> connectedBranches;
            private IEnumerable<Terminal> shunts;
            /// <summary>
            /// Returns all of the transmission elements attached to the DynamicTopologicalNode
            /// </summary>
            /// <param name="includeTransformers"></param>
            /// <returns></returns>
            public IEnumerable<Terminal> ConnectedBranches(bool includeTransformers)
            {
                
                if (connectedBranches == null)
                {
                    connectedBranches = new HashSet<Terminal>();
                    foreach (ConnectivityNode node in this.nodes.ConnectivityNodes.Values)
                    {
                        //have to create a new variable in order to prevent 
                        IEnumerable<Terminal> terms = node.Where
                            (x => x.ParentEquipment.EquipmentType == EquipmentTopoTypes.Conductor || x.ParentEquipment.Type == "TransformerEnd");
                        connectedBranches = connectedBranches.Union(terms);
                    }
                }
                return connectedBranches;
            }
            /// <summary>
            /// Returns an IEnumerable for each of the connected shunts
            /// </summary>
            /// <param name="IncludeTransformers"></param>
            /// <returns></returns>
            public IEnumerable<Terminal> CurrentInjectors(bool IncludeTransformers)
            {
                if (shunts == null)
                {
                    shunts = new HashSet<Terminal>();
                    foreach (ConnectivityNode node in this.nodes.ConnectivityNodes.Values)
                    {
                        var shuntEnumerable = node.Where(x => x.ParentEquipment.EquipmentType == EquipmentTopoTypes.Shunt);
                        shunts = shunts.Union(shuntEnumerable);
                    }
                }
                return shunts;
            }

            public DynamicTopologicalNode(ConnectivityNode firstConnectivityNode,VoltageLevel parent, DateTime time)
            {
                nodes.ConnectivityNodes.Add(firstConnectivityNode.Name, firstConnectivityNode);
                name = parent.name + "d" + parent.DynamicTopoNodeCount.ToString();
                parentStaticNode = parent;
                timeIndex = time;
            }
            
        }


    }
}
