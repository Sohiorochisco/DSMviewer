using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IdentifiedObjects;
using System.Drawing;
using System.Windows;
using System.IO;
using System.Windows.Shapes;
using ModelManipulation;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DSMviewer
{
    /// <summary>
    /// Wrapper for the IPowerSystemModel; builds the views for the user interface.
    /// </summary>
    internal class ViewHelper
    {
        /// <summary>
        /// Constructed on top of an IPowerSystemModel, which handles all interaction with system data.
        /// </summary>
        /// <param name="thisModel"></param>
        internal ViewHelper(IPowerSystemModel thisModel,double mapViewWidth, double mapViewHeight)
        {
            systemModel = thisModel;
            containers = systemModel.BuildModel();
            region = thisModel.GetRegion();
            AreaViewHeight = mapViewHeight;
            AreaViewWidth = mapViewWidth;
            determineRanges();
            MapIconHeight = 10;
            MapIconWidth = 10;
            iconPNGpaths = new Dictionary<string, string>();
            iconPNGpaths["Conductor"] = "..\\icons\\lineseg.png";
            iconPNGpaths["Transformer"] = "..\\icons\\transformer.png";
            iconPNGpaths["ConnectedBus"] = "..\\icons\\connectedbus.png";
            iconPNGpaths["Bus"] = "..\\icons\\bus.png";
            iconPNGpaths["NoConnectionBus"] = "..\\icons\\noconnectionbus.png";
            iconPNGpaths["OpenSwitch"] = "..\\icons\\openswitch.png";
            iconPNGpaths["ClosedSwitch"] = "..\\icons\\closedswitch.png";
            iconPNGpaths["Load"] = "..\\icons\\load.png";
            
        }
        public static ViewHelper LoadModel(string datasourcePath,double mapheight,double mapwidth)
        {
            var source = new DataFromExcel(datasourcePath);
            var model = new PowerSystemModel(source);
            var helper = new ViewHelper(model,mapwidth,mapheight);
   
            return helper;
        }

        public IEnumerable<string> ContainerNames()
        {
            return containers.Keys;

        }
        /// <summary>
        /// Generates the URL necessary to request a static satellite image of the system area through the Google Maps API
        /// </summary>
        /// <param name="areaPoints"></param>
        /// <returns></returns>
        internal  string GoogleMapsURL()
        {
            //Using https to avoid security issues/ warnings from Windows
            var urlBuilder = new StringBuilder("https://maps.googleapis.com/maps/api/staticmap?");
            urlBuilder.AppendFormat("center={0},{1}", centerYmap.ToString(), centerXmap.ToString());
            urlBuilder.AppendFormat("&zoom={0}", zoom.ToString());
            urlBuilder.Append("&size=512x512&scale=1&sensor=false");
            return urlBuilder.ToString();
        }
        internal IEnumerable<string> LocationNames()
        {
            var names = new HashSet<string>();
            foreach (Location location in region.Locations.Values)
            {
                names.Add(location.Name);
            }
            return names;
        }

        /// <summary>
        /// For a given location in the system, build the switchgear view.
        /// </summary>
        /// <param name="thisLocation"></param>
        /// <param name="iconSetter"></param>
        internal void BuildLocationView(Location thisLocation, SetGridIcon iconSetter)
        {
            var viewLayout = viewLayoutSet(thisLocation);
            var width = viewLayout[0].Count();
            string iconName;
            System.Windows.Controls.Image icon;
            
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    iconName = viewLayout[y][x];
                    icon = new System.Windows.Controls.Image();
                    icon.Source = new BitmapImage(new Uri(iconPNGpaths[iconName], UriKind.Relative));
                    iconSetter(y, x, icon);
                }
            }
        }

        /// <summary>
        /// Returns the icons, x and y coordinates for each of the Location icons to be shown over the map.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Ellipse> LocationOverlays()
        {
            var iconDefs = new HashSet<Ellipse>();
            Ellipse icon;
            double xcoord;
            double ycoord;
            ToolTip tt;
            foreach( Location location in region.Locations.Values)
            {
                icon = new Ellipse();
                icon.Height = MapIconHeight;
                icon.Width = MapIconWidth;
                icon.Visibility = Visibility.Visible;
                icon.Stretch = System.Windows.Media.Stretch.None;
                icon.Opacity = 1.0;
                //Defines a tooltip to display the name of the location during mouseover
                tt = new ToolTip();
                tt.Content = String.Format("Location {0}", location.Name);
                tt.PlacementTarget = icon;
                tt.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                icon.ToolTip = tt;

                //set icon coordinates for map display
                var coords = scaleToView(location.Position.XPosition, location.Position.YPosition);
                xcoord = coords.Key;
                ycoord = coords.Value;
                Canvas.SetLeft(icon, xcoord);
                Canvas.SetTop(icon, ycoord);
                iconDefs.Add(icon);               
            }
            return iconDefs;            
        }

        
        internal IEnumerable<System.Windows.Shapes.Line> LineOverlays(string containerName)
        {
            var container = containers[containerName];
            var lines = new HashSet<System.Windows.Shapes.Line>();
            System.Windows.Shapes.Line displayLine;
            PositionPoint point1, point2;
            Conductor conductor;
            KeyValuePair<double,double> p1coords;
            KeyValuePair<double,double> p2coords;
            ToolTip tt;
            foreach(Equipment equipment in container.Where(x => x.EquipmentType == EquipmentTopoTypes.Conductor))
            {
                conductor = equipment as Conductor;
                if (conductor != null)
                {
                    displayLine = new System.Windows.Shapes.Line();
                    point1 = conductor.Terminal1.Location.Position;
                    point2 = conductor.Terminal2.Location.Position;
                    p1coords = scaleToView(point1.XPosition, point1.YPosition);
                    p2coords = scaleToView(point2.XPosition, point2.YPosition);
                    displayLine.X1 = p1coords.Key;
                    displayLine.X2 = p2coords.Key;
                    displayLine.Y1 = p1coords.Value;
                    displayLine.Y2 = p2coords.Value;
                    tt = new ToolTip();
                    tt.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                    tt.Content = String.Format("Line Segment {0}", conductor.Name);
                    displayLine.ToolTip = tt;
                    lines.Add(displayLine);
                }

            }

            return lines;
        }

        internal double AreaViewHeight{set;private get;}
        internal double AreaViewWidth{set; private get;}
        internal double MapIconHeight {private get; set;}
        internal double MapIconWidth {private get; set;}




#region Private Members
        private IDictionary<string,EquipmentContainer> containers;
        private IPowerSystemModel systemModel;
        private SubGeographicalRegion region;
        private Dictionary<string, string> iconPNGpaths;

        //Used to determine the view dimensions for the area - viewer
        private double maxYcoord;
        private double maxXcoord;
        private double minYcoord;
        private double minXcoord;
        private double yscalor;
        private double xscalor;
        private double maxXmap;
        private double maxYmap;
        private double minXmap;
        private double minYmap;
        private double centerYmap;
        private double centerXmap;
        private int zoom;

        private void determineRanges()
        {
            //Set the mins at the max possible value, and the mins at the max possible value
            maxYcoord = -90;
            maxXcoord = -180;
            minYcoord = 90;
            minXcoord = 180;

            foreach(PositionPoint point in region.Positions.Values)
            {
                
                maxYcoord = Math.Max(maxYcoord,point.YPosition);
                maxXcoord = Math.Max(maxXcoord,point.XPosition);
                minXcoord = Math.Min(minXcoord,point.XPosition);
                minYcoord = Math.Min(minYcoord,point.YPosition);
            }

                //Determine the dimensions of the map to be received from Google

                var xRange = (maxXcoord - minXcoord);
                var yRange = (maxYcoord - minYcoord);
                centerXmap = (0.5 * xRange) + minXcoord;
                centerYmap = (0.5 * yRange) + minYcoord;

                //choose closest zoom guarenteed to contain all of the location points.
                zoom = (int)Math.Floor(Math.Log((Math.Min(180 / yRange, 360 / xRange)),2)) ; 
                var halfMapX = 180 / Math.Pow(2,zoom);
                var halfMapY = 90 / Math.Pow(2,zoom);
                maxXmap = centerXmap + halfMapX;
                maxYmap = centerYmap + halfMapY;
                minXmap = centerXmap - halfMapX;
                minYmap = centerYmap - halfMapY;
                xscalor = 0.5 * AreaViewWidth / (halfMapX);
                yscalor = 0.5 * AreaViewHeight / (halfMapY);
                zoom++; //Empirical decision based on the behavior of the map interface
        }

        private KeyValuePair<double,double> scaleToView(double xPosition,double yPosition)
        {
            double top;
            double left;
            top = (maxYmap - yPosition) * yscalor;
            left = (xPosition - minXmap) * xscalor;
            return new KeyValuePair<double,double>(left,top);
        }

        private string[][] viewLayoutSet(Location thisLocation)
        {
            //the number of icons along the x axis one the vacation view grid
            int gridWidth = (2 * thisLocation.Nodes.Count) - 1;
            string[][] viewLayout = new string[16][];
            int xindex = 0;
            int yindex;
            string nextIcon;
            Stack<Terminal> terms = new Stack<Terminal>();
            var equipmentShown = new Dictionary<int,Equipment>();
            Terminal currentTerm;
            IEnumerator<ConnectivityNode> nodeGetter = thisLocation.Nodes.GetEnumerator();
            ConnectivityNode currentNode = nodeGetter.Current;
            bool boolStore1;
            bool boolStore2;
            foreach (Terminal term in currentNode)
            {
                terms.Push(term);
            }
            //Initialize all rows of viewLayout, fill in first column
            for(yindex = 0; yindex < 16;yindex++)
            {
                viewLayout[yindex] = new String[gridWidth];
                if (terms.Count > 0)
                {
                    currentTerm = terms.Pop();
                    nextIcon = chooseNextIcon(currentTerm);
                    equipmentShown.Add(yindex,currentTerm.ParentEquipment);
                    viewLayout[yindex][0] = "ConnectedBus";
                    viewLayout[yindex][1] = nextIcon;
                }
                else
                {
                    viewLayout[yindex][0] = "Blank";
                    viewLayout[yindex][1] = "Blank";
                }
            }
            //
            while (nodeGetter.MoveNext())
            {
                xindex += 2;
                currentNode = nodeGetter.Current;
                foreach (Terminal term in currentNode)
                {
                    terms.Push(term);
                }
                for (yindex = 0; yindex < 16; yindex++)
                {
                    //If this row already has an equipment item with another connection to be made, draw the line
                    if (equipmentShown.Keys.Contains(yindex))
                    {
                        boolStore1 = equipmentShown[yindex].Type == "TransformerEnd";
                        boolStore2 = equipmentShown[yindex].EquipmentType == EquipmentTopoTypes.Switch;
                        if (boolStore1 || boolStore2)
                        {
                            if (currentNode.ContainsTerminalFor((ConductingEquipment)equipmentShown[yindex]))
                            {
                                viewLayout[yindex][xindex] = "ConnectedBus";
                                equipmentShown.Remove(yindex);
                            }
                            else
                            {
                                viewLayout[yindex][xindex] = "NoConnectionBus";
                            }
                        }
                        else
                        {
                            equipmentShown.Remove(yindex);
                            viewLayout[yindex][xindex] = "Bus";
                        }
                    }
                    else 
                    {
                        viewLayout[yindex][xindex] = "Bus";
                    }
                    
                }
                //Should occur if the number of equipment attached to a ConnectivityNode at a location is greater than 16.
                //To avoid having to throw this exception, in the future there should be some way to increase the size of the 
                //display grid given the maximum number of equipment attached to a node at a location, much as the width of 
                //the grid currently scales based on the number of ConnectivityNodes at the current location.
                if (terms.Count > 0)
                {
                    var exceptionString = String.Format
                        ("There is not enough room to display all of the equipment attached to node {0}", currentNode.Name);
                    throw new Exception(exceptionString);
                }
                
                
            }
            return viewLayout;                          
                            
 
        }

        /// <summary>
        /// Choose the next equipment icon to be displayed based on the next terminal off of the stack
        /// </summary>
        /// <param name="currentTerm"></param>
        /// <returns></returns>
        private String chooseNextIcon(Terminal currentTerm)
        {
            string nextIcon;
            switch (currentTerm.ParentEquipment.EquipmentType)
            {
                    
                case EquipmentTopoTypes.Conductor:
                    nextIcon = "Conductor";
                    break;
                case EquipmentTopoTypes.Switch:
                    var thisSwitch = currentTerm.ParentEquipment as Switch;
                    if (Equals(thisSwitch.SwitchState, SwitchState.closed))
                    { nextIcon = "ClosedSwitch"; }
                    else
                    { nextIcon = "OpenSwitch"; }
                    break;
                case EquipmentTopoTypes.Shunt:
                    switch (currentTerm.ParentEquipment.Type)
                    {
                        case "EnergyConsumer":
                            nextIcon = "Load";
                            break;
                        case "TransformerEnd":
                            nextIcon = "Transformer";
                            break;
                        default:
                            nextIcon = "Blank";
                            break;
                    }
                    break;
                case EquipmentTopoTypes.None:
                    nextIcon = "Blank";
                    break;
                default:
                    throw new Exception("The parent equipment doesn't seem to have a type");
            }
            return nextIcon;
        }

#endregion


        
    }

    /// <summary>
    /// Used to set the icons for the equipment and busses at a location.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="iconName"></param>
    /// <returns></returns>
    internal delegate bool SetGridIcon( int column, int row, System.Windows.Controls.Image image );
}
