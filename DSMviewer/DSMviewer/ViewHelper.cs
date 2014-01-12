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

namespace DSMviewer{
    /// <summary>
    /// Wrapper for the IPowerSystemModel; builds the views for the user interface.
    /// </summary>
    internal class ViewHelper{
        /// <summary>
        /// Constructed on top of an IPowerSystemModel, which handles all interaction with system data.
        /// </summary>
        /// <param name="thisModel"></param>
        internal ViewHelper(IPowerSystemModel thisModel,double mapViewWidth, double mapViewHeight){
            systemModel = thisModel;
            containers = systemModel.BuildModel();
            region = thisModel.GetRegion();
            AreaViewHeight = mapViewHeight;
            AreaViewWidth = mapViewWidth;
            determineRanges();           
        }
        public static ViewHelper LoadModel(string datasourcePath,double mapheight,double mapwidth){
            var source = new DataFromExcel(datasourcePath);
            var model = new PowerSystemModel(source);
            var helper = new ViewHelper(model,mapwidth,mapheight);
            return helper;
        }

        public IEnumerable<string> ContainerNames() {
            return containers.Keys;
        }
        /// <summary>
        /// Generates the URL necessary to request a static satellite image of the system area through the Google Maps API
        /// </summary>
        /// <param name="areaPoints"></param>
        /// <returns></returns>
        internal  BitmapImage GoogleMapsURL(){
            //Using https to avoid security issues/ warnings from Windows
            var urlBuilder = new StringBuilder("https://maps.googleapis.com/maps/api/staticmap?");
            urlBuilder.AppendFormat("center={0},{1}", centerYmap.ToString(), centerXmap.ToString());
            urlBuilder.AppendFormat("&zoom={0}", zoom.ToString());
            urlBuilder.Append("&size=512x512&scale=1&sensor=false");
            BackgroundMap = new BitmapImage(new Uri(urlBuilder.ToString(),UriKind.Absolute));
            return BackgroundMap;
        }
        internal BitmapImage BackgroundMap{get;private set;}
        /// <summary>
        /// Returns a string giving the name for each location in the associated SubGeographicalRegion
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<string> LocationNames(){
            var names = new HashSet<string>();
            foreach (Location location in region.Locations.Values){
                names.Add(location.Name);
            }
            return names;
        }

        /// <summary>
        /// Updates the collection of Location definition objects to be displayed in the area view
        /// </summary>
        /// <returns></returns>
        internal LocationDisplays LocationOverlays(){
            var iconDefs = new LocationDisplays();
            LocationDef icon;
            double xcoord;
            double ycoord;
            foreach( Location location in region.Locations.Values){
                icon = new LocationDef();            
                icon.Description = String.Format("Location {0}", location.Name);
                //set icon coordinates for map display
                var coords = scaleToView(location.Position.XPosition, location.Position.YPosition);
                xcoord = coords.Key;
                ycoord = coords.Value;
                icon.X = xcoord - 6;
                icon.Y = ycoord - 6;
                icon.BackingLocation = location;
                iconDefs.Add(icon);               
            }
            return iconDefs;            
        }
        

        
        internal LineDisplays LineOverlays(string containerName){
            var container = containers[containerName];
            var lines = new LineDisplays();
            LineDef displayLine;
            PositionPoint point1, point2;
            Conductor conductor;
            KeyValuePair<double,double> p1coords;
            KeyValuePair<double,double> p2coords;
            foreach(Equipment equipment in container.Where(x => x.EquipmentType == EquipmentTopoTypes.Conductor)){
                conductor = equipment as Conductor;
                if (conductor != null){
                    displayLine = new LineDef();
                    point1 = conductor.Terminal1.Location.Position;
                    point2 = conductor.Terminal2.Location.Position;
                    p1coords = scaleToView(point1.XPosition, point1.YPosition);
                    p2coords = scaleToView(point2.XPosition, point2.YPosition);
                    displayLine.X1 = p1coords.Key;
                    displayLine.X2 = p2coords.Key;
                    displayLine.Y1 = p1coords.Value;
                    displayLine.Y2 = p2coords.Value;
                    displayLine.Description = String.Format("Line Segment {0}", conductor.Name); 
                    lines.Add(displayLine);
                }
            }
            return lines;
        }
        internal LineDisplays LineDefs { get; private set; }
        internal double AreaViewHeight{set;private get;}
        internal double AreaViewWidth{set; private get;}
        internal double MapIconHeight {private get; set;}
        internal double MapIconWidth {private get; set;}




#region Private Members
        private IDictionary<string,EquipmentContainer> containers;
        private IPowerSystemModel systemModel;
        private SubGeographicalRegion region;

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

        /// <summary>
        /// Determines the proper coordinates and dimensions for each object displayed in the map view.
        /// </summary>
        private void determineRanges(){
            //Set the mins at the max possible value, and the mins at the max possible value
            maxYcoord = -90;
            maxXcoord = -180;
            minYcoord = 90;
            minXcoord = 180;

            foreach(PositionPoint point in region.Positions.Values){ 
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
                zoom = (int)Math.Floor(Math.Log((Math.Min(180 / yRange, 360 / xRange)),2)); 
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

        private KeyValuePair<double,double> scaleToView(double xPosition,double yPosition){
            double top;
            double left;
            top = (maxYmap - yPosition) * yscalor;
            left = (xPosition - minXmap) * xscalor;
            return new KeyValuePair<double,double>(left,top);
        }


             

#endregion


        
    }


}
