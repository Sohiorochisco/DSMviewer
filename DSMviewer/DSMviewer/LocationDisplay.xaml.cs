using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IdentifiedObjects;

namespace DSMviewer
{
    /// <summary>
    /// Interaction logic for LocationDisplay.xaml
    /// </summary>
    public partial class LocationDisplay : UserControl
    {
        public LocationDisplay(SchematicMaker sm)
        {
            DataContext = sm;
            InitializeComponent();
        }
    }
}
