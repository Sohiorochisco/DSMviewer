using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IdentifiedObjects
{
    public class Location : IdentifiedObject
    {
        /// <summary>
        /// Gives the Equipment at the location 
        /// </summary>
        internal HashSet<Terminal> Terminals{get; private set;}

        /// <summary>
        /// Gives the ConnectivityNodes at a Location
        /// </summary>
        internal HashSet<ConnectivityNode> Nodes { get; private set; }



        public PositionPoint Position { get; private set; }

        /// <summary>
        /// Creates a Location at the given PositionPoint thisPosition
        /// </summary>
        /// <param name="thisposition"></param>
        public Location(DataRow row, PositionPoint thisPosition)
            :base(row)
        {
            Position = thisPosition;
            Terminals = new HashSet<Terminal>();
            Nodes = new HashSet<ConnectivityNode>();
        }

        public void AddTerminal(Terminal term)
        {
            Terminals.Add(term);
            Nodes.Add(term.ConnectionPoint);
        }
    }

    

    
}
