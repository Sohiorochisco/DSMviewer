using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IdentifiedObjects
{
    public class ConnectivityNodeContainer : IdentifiedObject
    {
        //protected List<ConnectivityNode> connectivityNodes;
        protected Dictionary<string, ConnectivityNode> connectivityNodes;
        public IDictionary<string,ConnectivityNode> ConnectivityNodes
        {
            get { return (IDictionary<string,ConnectivityNode>)connectivityNodes; }
        }


        public ConnectivityNodeContainer(string newName) 
        {
            name = newName;
            connectivityNodes = new Dictionary<string,ConnectivityNode>();
        }
         
    }
}
