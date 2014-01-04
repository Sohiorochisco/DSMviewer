using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DSMviewer
{
    //Defines classes to serve as sources for the area display window.
    public class LineDef
    {
        public double X1 { get; set; }
        public double X2 { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }
        public string Description { get; set; }
        internal LineDef() { }
    }
    public class LocationDef
    {
        internal LocationDef() { }
        public double X { get; set; }
        public double Y { get; set; }
        public string Description { get; set; }
    }
    public class LocationDisplays : ObservableCollection<LocationDef>
    { }
    public class LineDisplays : ObservableCollection<LineDef>
    { }

}
