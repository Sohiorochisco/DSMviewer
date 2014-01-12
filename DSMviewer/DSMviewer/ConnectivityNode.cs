using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace IdentifiedObjects{
    /// <summary>
    /// Collection of Terminals, signifying a single connection point for multiple ConductingEquipments
    /// </summary>
    public class ConnectivityNode : IdentifiedObject, IEnumerable<Terminal>{
        
        public TopologicalNode.VoltageLevel StaticTopoNode {get;internal set;}

        /// <summary>
        /// gives the group of currently connected points at the same potential
        /// </summary>
        public TopologicalNode.DynamicTopologicalNode DynamicTopoNode { get; internal set; }

       
        /// <summary>
        /// Contains all voltages at the node
        /// </summary>
        public Voltages NodeVoltages { get;set; }

        public int TerminalCount {
            get { return connections.Count; }
        }
        //May modify to fully implement ICollection
        protected List<Terminal> connections;
        public IEnumerator<Terminal> GetEnumerator(){
            return connections.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return connections.GetEnumerator();
        }
        public void Add(Terminal term){
            connections.Add(term);
        }
    //indexer for ease of access
        public Terminal this[int index]{
            get{ return connections[index]; }
            set{ connections[index] = value;}
        }
        /// <summary>
        /// Returns a list of all ConnectivityNodes connected to the current node by switches
        /// </summary>
        /// <returns></returns>
        public LinkedList<ConnectivityNode> AdjacentSwitchNodes(bool includeOpen){
            var adjacentNodes = new LinkedList<ConnectivityNode>();
            foreach (Terminal term in this.connections){
                if (term.ParentEquipment.EquipmentType == EquipmentTopoTypes.Switch){  
                    var parentGear = (Switch)term.ParentEquipment;
                    if ((parentGear.SwitchState == SwitchState.closed) | includeOpen){
                        if (parentGear.Terminal1 == term){
                            adjacentNodes.AddLast(parentGear.Terminal2.ConnectionPoint);
                        }
                        if (parentGear.Terminal2 == term){
                            adjacentNodes.AddLast(parentGear.Terminal2.ConnectionPoint);
                        }
                    }
                }
            }
            return adjacentNodes;
        }

        /// <summary>
        /// Returns a list of all ConnectivityNodes connected to the current node by conductors
        /// </summary>
        /// <returns></returns>
        public LinkedList<ConnectivityNode> AdjacentConductorNodes(bool includeTransformers){
            var adjacentNodes = new LinkedList<ConnectivityNode>();
            foreach (Terminal term in this.connections){
                if (term.ParentEquipment.EquipmentType == EquipmentTopoTypes.Conductor) {
                    var parentGear = (Switch)term.ParentEquipment;
                    if (parentGear.Terminal1 == term){
                        adjacentNodes.AddLast(parentGear.Terminal2.ConnectionPoint);
                    }
                    if (parentGear.Terminal2 == term){
                        adjacentNodes.AddLast(parentGear.Terminal2.ConnectionPoint);
                    }
                }
                if (includeTransformers && term.ParentEquipment.Type == "TransformerEnd"){
                    var parentEnd = (PowerTransformer.TransformerEnd)term.ParentEquipment;
                    var parent = parentEnd.ParentTransformer;
                    foreach (PowerTransformer.TransformerEnd end in parent.Ends.Where(x => !(x.Terminal1 == term))){
                        adjacentNodes.AddLast(end.Terminal1.ConnectionPoint);
                    }
                }
            }
            return adjacentNodes;
        }

        public bool ContainsTerminalFor(ConductingEquipment thisEquipment){
            if (thisEquipment.EquipmentType == EquipmentTopoTypes.Switch){ 
                var thisSwitch = thisEquipment as Switch;
                if (connections.Contains(thisSwitch.Terminal2)){
                    return true;
                }
            }
            if (thisEquipment.EquipmentType == EquipmentTopoTypes.Conductor){
                var conductor = thisEquipment as Conductor;
                if (connections.Contains(conductor.Terminal2)) {
                    return true;
                }
            }
            if (thisEquipment.Type == "TransformerEnd"){
                var tEnd = thisEquipment as PowerTransformer.TransformerEnd;
                var tTerms = tEnd.ParentTransformer.Terminals();
                foreach (Terminal thisTerm in tTerms){
                    if (connections.Contains(thisTerm)){
                        return true;
                    }
                }    
            }
            if (connections.Contains(thisEquipment.Terminal1)){ return true;}
            return false; 
        }

        /// <summary>
        /// Although not required by CIM, this model requires at least one piece of conducting equipment be connected to each node.
        /// </summary>
        /// <param name="first"></param>
        public ConnectivityNode(string newName, Terminal first) {
            name = newName;
            connections = new List<Terminal>();           
            connections.Add(first);
            NodeVoltages = new Voltages(first.Phases);
            this.type = "ConnectivityNode";
        }
    }
}
