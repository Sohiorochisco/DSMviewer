using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IdentifiedObjects;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace DSMviewer
{
    /*********************************************************************************
     * Important
     * ---------
     * This class primarily implements an algorithm for generating a circuit schematic,
     * and involves some nasty procedural code. Several optimizations and improvements 
     * may be possible, but please tread carefully and do not break what already works.     
    ***********************************************************************************/
    /// <summary>
    /// Builds and maintains the backing collection for the Location schematic display
    /// </summary>
    public class SchematicMaker
    {
        /// <summary>
        /// Builds and maintains the backing collection for the location schematic display
        /// </summary>
        /// <param name="location">
        /// Location to be displayed
        /// </param>
        /// <param name="w"> 
        /// The minimum width for the uniform grid display
        /// </param>
        /// <param name="h">
        /// The minimum height for the uniform grid display
        /// </param>
        public SchematicMaker(Location location, int w, int h){
            thisLocation = location;
            setview(w, h);
        }

        public BindingList<LocationIcon> IconList{private set; get;}
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int GridWidth { get { return 50 * Width - 1; } }
        public int GridWidthMax { get { return 50 * Width + 5; } }

        /// <summary>
        /// Returns a Uri corresponding to the icon for a given object name
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        static public Uri GetObjectImage(string n){
            return IconGetter.Instance.RetrieveIcon(n);
        }

        /// <summary>
        /// Generates the backing collection for the location view.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void setview(int w, int h){
            nodeColumns = new Dictionary<ConnectivityNode,int>();
            var bs =  thisLocation.Nodes;
            var enu = bs.GetEnumerator();
            for(int c = 0; c < bs.Count; c++){
                if (enu.MoveNext()){
                    nodeColumns.Add(enu.Current, c * 2); 
                }
            }
            var expDim = (bs.Count * 2) - 1;
            var tempT = emptyView(bs.Count, w, h);
            string[,] icns = tempT.Item1;
            IdentifiedObject[,] refs = tempT.Item2;
            //cns contains equipment who have two terminals at the location
            var cns = new Dictionary<Terminal, Equipment>();
            //sns contains equipment which have only a single terminal at this location
            var sns = new Dictionary<Terminal, Equipment>();
            foreach (ConnectivityNode node in bs){
                foreach (Terminal term in node){
                    var kvp = getParentEquipment(term);
                    if (kvp.Value) { // indicates whether the equipment should have multiple terms at this location
                        cns.Add(term, kvp.Key); 
                    }else{
                        sns.Add(term, kvp.Key);
                    }
                }
            }
            //Currently, once a row has been used in any way, none of the remaining blanks left of the 
            //used portion will be filled in. This is much easier to implement than attempting to fill
            //in the remaining spots to the left. The only fill-in will tend to occur in the upper right 
            //portion of the display
            var numUsed = new int[Height];
            int cnum = 0;
            int rw;
            for(cnum = 0; cnum < w; cnum += 2){
                rw = 0;
                var ienm = cns.Where(x => nodeColumns[x.Key.ConnectionPoint] == cnum);
                var temps = ienm.ToArray();//forces enumeration(takes time; not sure how to avoid)
                int count = temps.Count();
                //Add the visual representation of each of the connections which start at this column
                for(int i = 0; i < count; i++){
                    var kvp = temps[i];
                    bool used = false;
                    while (used == false){
                        if (numUsed[rw] > cnum){
                            rw++;
                        }else{
                            //Not quicksort, but the numbers here are so small that this is quicker.
                            Terminal t2 = getMatch(cns, kvp.Key);
                            numUsed[rw] = fillInConnection(kvp.Key, t2, kvp.Value, icns, rw, refs);
                            used = true;
                            rw++;
                        }
                    }
                }
                //Then fill in all of the single-terminal equipment using the remaining space.
                foreach (KeyValuePair<Terminal, Equipment> kv in sns.Where(x => nodeColumns[x.Key.ConnectionPoint] == cnum)){
                    bool used = false;
                    while (used == false){
                        if (numUsed[rw] > cnum){
                            rw++;
                        }else{
                            numUsed[rw] = fillInSingle(kv.Key, kv.Value, icns, rw,refs);
                            used = true;
                        }
                    }
                }
            }
            //Finally, send the results to the list which serves as a backing collection for the display
            var iconList = new List<LocationIcon>(Width * Height);
            var getter = IconGetter.Instance;
            for (int y = 0; y < Height; y++){
                int startIndex = y  * Width;
                for (int x = 0; x < Width; x++){
                    var icon = new LocationIcon();
                    icon.BackingObject = refs[y, x];
                    getter.AddIcon(icns[y, x]);
                    icon.Icon = getter.RetrieveIcon(icns[y, x]);
                    iconList.Add(icon);
                }
            }
            
            IconList = new BindingList<LocationIcon>(iconList);
        }

        /// <summary>
        /// Draws the Equipment and wires which span between terminals.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="pe"></param>
        /// <param name="icns"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        int fillInConnection(Terminal t1, Terminal t2, Equipment pe, string[,] icns, int r, IdentifiedObject[,] refs){
            int st = nodeColumns[t1.ConnectionPoint];
            int en = nodeColumns[t2.ConnectionPoint];
            icns[r, st] = "connectedbus";
            icns[r, en] = "connectedbus";
            icns[r, st + 1] = String.Format("{0}{1}", pe.Type, pe.State());
            refs[r, st + 1] = pe;
            for(int i = st + 2; i < en; i++){
                icns[r, i] = (icns[r, i] == "bus") ? "noconnectionbus" : "wire";
            }
            return en;
        }
        /// <summary>
        /// Adds the icon and connection for a piece of equipment that has only one terminal at the location
        /// </summary>
        /// <param name="t"></param>
        /// <param name="icns"></param>
        /// <param name="r"></param>
        int fillInSingle(Terminal t, Equipment eq, string[,] icns, int r, IdentifiedObject[,] refs){
            var c = nodeColumns[t.ConnectionPoint];
            icns[r, c] = "connectedbus";
            icns[r, c + 1] = String.Format("{0}{1}", eq.Type, eq.State());
            refs[r, c + 1] = eq;
            return c + 2;
        }
        /// <summary>
        /// Returns the terminal with the same conducting equipment as Terminal t.
        /// </summary>
        /// <param name="umts"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        Terminal getMatch(Dictionary<Terminal, Equipment> umts, Terminal t){
            Equipment pe = getParentEquipment(t).Key;
            umts.Remove(t);
            var kv = umts.Single(x => Equals(x.Value, pe));
            umts.Remove(kv.Key);
            return kv.Key;
        }

        /// <summary>
        /// Returns a definition for a view with no connections between busses
        /// </summary>
        /// <param name="busCount"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Tuple<string[,],IdentifiedObject[,]> emptyView(int busCount, int width, int height){
            var expDim = (2 * busCount);
            var expH = thisLocation.Terminals.Count;
            Width = Math.Max(expDim, width);
            Height = Math.Max(expH, height);
            var icns = new string[Height, Width];
            var refs = new IdentifiedObject[Height, Width];            
            //Draw each bus and the adjoining space at the same time
            var bw = busCount * 2 - 1;
            for (int i = 0; i <= bw; i += 2){
                ConnectivityNode thisref = nodeColumns.Single(x => x.Value == i).Key;
                for (int j = 0; j < Height; j++){
                    icns[j, i] = "bus";
                    icns[j, i + 1] = "blank";
                    refs[j, i] = thisref;
                }
            }
            for (int i = bw; i < Width; i++){
                for (int j = 0; j < Height; j++){
                    icns[j, i] = "blank";
                }
            }
            return new Tuple<string[,],IdentifiedObject[,]>(icns,refs);
        }


        /// <summary>
        /// Returns the equipment associated with a terminal, along with a bool indicated whether
        /// the equipment should have another terminal at the same location.
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        private KeyValuePair<Equipment, bool> getParentEquipment(Terminal term){
            if (term.ParentEquipment.Type == "TransformerEnd"){
                var t = term.ParentEquipment as PowerTransformer.TransformerEnd;
                return new KeyValuePair<Equipment, bool>(t.ParentTransformer, true);
            }else if (term.ParentEquipment.EquipmentType == EquipmentTopoTypes.Switch){ 
                return new KeyValuePair<Equipment, bool>(term.ParentEquipment, true);
            }
            return new KeyValuePair<Equipment, bool>(term.ParentEquipment, false);
        }

        /// <summary>
        /// Stores the column numbers for each of the ConnectivityNodes at the location.
        /// </summary>
        private Dictionary<ConnectivityNode, int> nodeColumns;

        /// <summary>
        /// The location for which this class serves as a wrapper around.
        /// </summary>
        private Location thisLocation;

        /// <summary>
        /// Singleton class for managing icons for the location display
        /// </summary>
        private sealed class IconGetter{
            private static IconGetter instance = new IconGetter();
            static IconGetter() { }
            private IconGetter() { 
                icons = new Dictionary<string,Uri>();
            }
            private Dictionary<string, Uri> icons;
            public static IconGetter Instance { get { return instance; } }

            public void AddIcon(string iconName){
                if(!icons.ContainsKey(iconName)){
                    var iconUri = String.Format("..\\..\\..\\..\\icons\\{0}.png", iconName);
                    var iconAbsUri = System.IO.Path.GetFullPath(iconUri);
                    icons[iconName] = new Uri(@iconAbsUri, UriKind.Absolute);
                }
                return;
            }
            public Uri RetrieveIcon(string iconName){
                if (!icons.ContainsKey(iconName))
                {
                    var iconUri = String.Format("..\\..\\..\\..\\icons\\{0}.png", iconName);
                    var iconAbsUri = System.IO.Path.GetFullPath(iconUri);
                    icons[iconName] = new Uri(@iconAbsUri, UriKind.Absolute);
                }
                return icons[iconName];
            }

        }


    }
}
